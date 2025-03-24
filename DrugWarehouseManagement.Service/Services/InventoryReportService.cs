using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using OfficeOpenXml;
using QuestPDF.Infrastructure;
using QuestPDF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using DrugWarehouseManagement.Service.DTO.Response;
using QuestPDF.Helpers;

namespace DrugWarehouseManagement.Service.Services
{
    public class InventoryReportService : IInventoryReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<byte[]> ExportInventoryReportPdf(int warehouseId, Instant startDate, Instant endDate)
        {
            // ==========================
            // 1) TÍNH TOÁN SỐ LIỆU
            // ==========================

            // (a) Lấy danh sách sản phẩm có trong kho
            var productsQuery = _unitOfWork.LotRepository
                .GetAll()
                .Where(l => l.WarehouseId == warehouseId)
                .Select(l => l.Product)
                .Distinct();
            var products = await productsQuery.ToListAsync();

            // (b) Đầu kỳ (OpeningStock) - inbound có InboundRequestId != null, trước startDate
            var openingStockDict = await _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                .Where(d => d.Inbound.WarehouseId == warehouseId
                            && d.Inbound.InboundDate < startDate
                            && d.Inbound.Status == InboundStatus.Completed
                            && d.Inbound.InboundRequestId != null)
                .GroupBy(d => d.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    // Lấy OpeningStock của lần inbound gần nhất
                    OpeningStock = g.OrderByDescending(x => x.Inbound.InboundDate)
                                    .FirstOrDefault().OpeningStock
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.OpeningStock);

            // ------------------------ NHẬP ------------------------

            // (c1) Nhập Mua (Inbound có InboundRequestId != null, trong [startDate, endDate])
            var inboundBuy = await _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                .Where(d => d.Inbound.WarehouseId == warehouseId
                         && d.Inbound.InboundDate >= startDate
                         && d.Inbound.InboundDate <= endDate
                         && d.Inbound.Status == InboundStatus.Completed
                         && d.Inbound.InboundRequestId != null)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // (c2) Nhập Chuyển (LotTransfer to warehouseId)
            var transferIn = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                .Include(d => d.Lot) // cần Include(d => d.Lot.ProductId) => tuỳ
                .Where(d => d.LotTransfer.ToWareHouseId == warehouseId
                            && d.LotTransfer.CreatedAt >= startDate
                            && d.LotTransfer.CreatedAt <= endDate
                            && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed)
                .GroupBy(d => d.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // (c3) Nhập trả về  -inbound không có InboundRequestId
          
            var inboundReturn = await _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                .Where(d => d.Inbound.WarehouseId == warehouseId
                         && d.Inbound.InboundDate >= startDate
                         && d.Inbound.InboundDate <= endDate
                         && d.Inbound.Status == InboundStatus.Completed
                         && d.Inbound.InboundRequestId == null)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // ------------------------ XUẤT ------------------------

            // (d1) Xuất Bán
            var outboundSell = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Include(od => od.Lot)
                .Where(od => od.Lot.WarehouseId == warehouseId
                             && od.Outbound.OutboundDate >= startDate
                             && od.Outbound.OutboundDate <= endDate
                             && od.Outbound.Status == OutboundStatus.Completed)
                .GroupBy(od => od.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // (d2) Xuất Chuyển (LotTransfer from warehouseId)
            var transferOut = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                .Include(d => d.Lot)
                .Where(d => d.LotTransfer.FromWareHouseId == warehouseId
                            && d.LotTransfer.CreatedAt >= startDate
                            && d.LotTransfer.CreatedAt <= endDate
                            && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed)
                .GroupBy(d => d.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // ------------------------ Tạo dictionary ------------------------
            var buyDict = inboundBuy.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferInDict = transferIn.ToDictionary(x => x.ProductId, x => x.Qty);
            var inboundReturnDict = inboundReturn.ToDictionary(x => x.ProductId, x => x.Qty);

            var sellDict = outboundSell.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferOutDict = transferOut.ToDictionary(x => x.ProductId, x => x.Qty);

            // ------------------------ Build list<InventoryReportRow> ------------------------
            var reportData = new List<InventoryReportRow>();

            foreach (var p in products)
            {
                int pid = p.ProductId;

                int beginning = openingStockDict.ContainsKey(pid) ? (openingStockDict[pid] ?? 0) : 0;
                int buyQty = buyDict.ContainsKey(pid) ? buyDict[pid] : 0;
                int inTransQty = transferInDict.ContainsKey(pid) ? transferInDict[pid] : 0;
                int inReturnQty = inboundReturnDict.ContainsKey(pid) ? inboundReturnDict[pid] : 0;

                int sellQty = sellDict.ContainsKey(pid) ? sellDict[pid] : 0;
                int outTransQty = transferOutDict.ContainsKey(pid) ? transferOutDict[pid] : 0;

                int remain = beginning
                             + (buyQty + inTransQty + inReturnQty)
                             - (sellQty + outTransQty);

                reportData.Add(new InventoryReportRow
                {
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    SKU = p.SKU,  // hoặc p.UnitName, tuỳ logic
                    Beginning = beginning,
                    BuyQty = buyQty,
                    TransferInQty = inTransQty,
                    ReturnInQty = inReturnQty,
                    SellQty = sellQty,
                    TransferOutQty = outTransQty,                 
                    Remain = remain
                });
            }

            // ==========================
            // 2) XUẤT RA FILE PDF
            // ==========================

            // Lấy tên kho, format ngày
            var warehouse = await _unitOfWork.WarehouseRepository
                                    .GetByWhere(w => w.WarehouseId == warehouseId)
                                    .FirstOrDefaultAsync();
            string warehouseName = warehouse?.WarehouseName ?? "N/A";
            string startDateStr = startDate.ToDateTimeUtc().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            string endDateStr = endDate.ToDateTimeUtc().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            // Kích hoạt license QuestPDF (nếu dùng Community)
            Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Tuỳ chỉnh size trang, lề
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // ---------------- HEADER ----------------
                    page.Header().Column(col =>
                    {
                        col.Item().Text("CÔNG TY TNHH DƯỢC PHẨM Trung Hạnh").Bold().FontSize(14).AlignCenter();
                        col.Item().Text("BÁO CÁO NHẬP XUẤT TỒN").Bold().FontSize(16).AlignCenter();
                        col.Item().Text($"Kho: {warehouseName}").AlignCenter();
                        col.Item().Text($"Từ ngày {startDateStr} đến ngày {endDateStr}").AlignCenter();
                    });

                    // ---------------- CONTENT ----------------
                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            // Định nghĩa cột
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25); // STT
                                columns.RelativeColumn(2);  // Mã hàng
                                columns.RelativeColumn(3);  // Tên hàng
                                columns.RelativeColumn(2);  // ĐVT
                                columns.RelativeColumn(1);  // Đầu kỳ
                                columns.RelativeColumn(1);  // Mua
                                columns.RelativeColumn(1);  // Chuyển (Nhập)
                                columns.RelativeColumn(1);  // Trả về (Nhập)
                                columns.RelativeColumn(1);  // Bán
                                columns.RelativeColumn(1);  // Chuyển (Xuất)
                                columns.RelativeColumn(1);  // Tồn
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Border(1).AlignCenter().Text("STT").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Mã hàng").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Tên hàng").Bold();
                                header.Cell().Border(1).AlignCenter().Text("ĐVT").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Đầu kỳ").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Mua").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Chuyển\n(Nhập)").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Trả về\n(Nhập)").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Bán").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Chuyển\n(Xuất)").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Tồn").Bold();
                            });

                            int stt = 1;
                            foreach (var r in reportData)
                            {
                                table.Cell().Border(1).AlignCenter().Text(stt);
                                table.Cell().Border(1).Text(r.ProductCode);
                                table.Cell().Border(1).Text(r.ProductName);
                                table.Cell().Border(1).AlignCenter().Text(r.SKU);
                                table.Cell().Border(1).AlignRight().Text(r.Beginning.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.BuyQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.TransferInQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.ReturnInQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.SellQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.TransferOutQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.Remain.ToString("N0"));

                                stt++;
                            }
                        });
                    });            
                });
            }).GeneratePdf();

            // Trả về mảng byte PDF
            return pdfBytes;
        }
    }
}
