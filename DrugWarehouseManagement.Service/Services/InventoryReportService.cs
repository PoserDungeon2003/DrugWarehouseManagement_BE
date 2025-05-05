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
using DrugWarehouseManagement.Repository.Models;
using OpenCvSharp.Detail;

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

            // 1. Tồn đầu kỳ: với mỗi Lot, chọn Quantity của transaction lần cuối <= startDate
            var openingByLot = await _unitOfWork.InventoryTransactionRepository
                .GetAll()
                .Where(t => t.Lot.WarehouseId == warehouseId
                         && t.CreatedAt <= startDate)
                .GroupBy(t => t.LotId)
                .Select(g => new
                {
                    LotId = g.Key,
                    // lấy Quantity của record mới nhất
                    Quantity = g.OrderByDescending(x => x.CreatedAt)
                                .ThenByDescending(x => x.Id)
                                .FirstOrDefault().Quantity
                })
                .ToListAsync();
            // Chuyển từ per-lot thành per-product
            var openingStockDict = openingByLot
                .GroupBy(x => _unitOfWork.LotRepository
                                    .GetAll()
                                    .Where(l => l.LotId == x.LotId)
                                    .Select(l => l.ProductId)
                                    .FirstOrDefault())
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Quantity)
                );

            // 2. Tồn cuối kỳ: tương tự, mốc endDate
            var closingByLot = await _unitOfWork.InventoryTransactionRepository
                .GetAll()
                .Where(t => t.Lot.WarehouseId == warehouseId
                         && t.CreatedAt <= endDate)
                .GroupBy(t => t.LotId)
                .Select(g => new
                {
                    LotId = g.Key,
                    Quantity = g.OrderByDescending(x => x.CreatedAt)
                                .ThenByDescending(x => x.Id)
                                .FirstOrDefault().Quantity
                })
                .ToListAsync();

            var closingStockDict = closingByLot
                .GroupBy(x => _unitOfWork.LotRepository
                                    .GetAll()
                                    .Where(l => l.LotId == x.LotId)
                                    .Select(l => l.ProductId)
                                    .FirstOrDefault())
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Quantity)
                );

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

            // (c2) Nhập chuyển thông thường (chuyển từ các kho khác ngoài kho 6)
            var transferInNormal = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                .Include(d => d.Lot)
                .Where(d => d.LotTransfer.ToWareHouseId == warehouseId
                          && d.LotTransfer.FromWareHouseId != 6  // Các kho khác ngoài kho 6
                          && d.LotTransfer.CreatedAt >= startDate
                          && d.LotTransfer.CreatedAt <= endDate
                          && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed)
                .GroupBy(d => d.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // (c3) Nhập trả về: lấy từ giao dịch chuyển kho từ kho 6 sang kho đích
            var transferInReturn = await _unitOfWork.LotTransferDetailsRepository
                 .GetAll()
                 .Include(d => d.LotTransfer)
                 .Include(d => d.Lot)
                 .Where(d => d.LotTransfer.ToWareHouseId == warehouseId
                             && d.LotTransfer.FromWareHouseId == 6  // Chỉ lấy từ kho tạm (6)
                             && d.LotTransfer.CreatedAt >= startDate
                             && d.LotTransfer.CreatedAt <= endDate
                             && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed)
                 .GroupBy(d => d.Lot.ProductId)
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
                     && od.TotalPrice > 0
                     && (od.Outbound.Status == OutboundStatus.Completed || od.Outbound.Status == OutboundStatus.Returned))
                .GroupBy(od => od.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // (d2) Xuất chuyển (chuyển ra từ kho đích)
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

            // (d3) Xuất mẫu (Outbound có TotalPrice = 0)
            var sampleExport = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Include(od => od.Lot)
                .Where(od => od.Lot.WarehouseId == warehouseId
                             && od.Outbound.OutboundDate >= startDate
                             && od.Outbound.OutboundDate <= endDate
                             && (od.Outbound.Status == OutboundStatus.Completed || od.Outbound.Status == OutboundStatus.Returned)
                             && od.TotalPrice == 0)
                .GroupBy(od => od.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();
            // (d4) Xuất Hư (Báo cáo kiểm kê hàng hư)
            var outboundDamage = await _unitOfWork.InventoryCheckRepository
                .GetByWhere(ic => ic.WarehouseId == warehouseId &&
                      ic.CheckDate >= startDate &&
                      ic.CheckDate <= endDate)
                .Include(ic => ic.InventoryCheckDetails)
                .ThenInclude(icd => icd.Lot)
                .SelectMany(ic => ic.InventoryCheckDetails)
                .Where(icd => icd.Status == InventoryCheckStatus.Damaged)
                .GroupBy(icd => icd.Lot.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Qty = g.Sum(icd => icd.CheckQuantity ?? 0)
                })
                .ToListAsync();
            // (d5) Xuất Mất (Báo cáo kiểm kê hàng mất)
            var outboundLost = await _unitOfWork.InventoryCheckRepository
                .GetByWhere(ic => ic.WarehouseId == warehouseId &&
                      ic.CheckDate >= startDate &&
                      ic.CheckDate <= endDate)
                .Include(ic => ic.InventoryCheckDetails)
                .ThenInclude(icd => icd.Lot)
                .SelectMany(ic => ic.InventoryCheckDetails)
                .Where(icd => icd.Status == InventoryCheckStatus.Lost)
                .GroupBy(icd => icd.Lot.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Qty = g.Sum(icd => icd.CheckQuantity ?? 0)
                })
                .ToListAsync();

            // ------------------------ Tạo dictionary ------------------------

            var buyDict = inboundBuy.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferInNormalDict = transferInNormal.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferInReturnDict = transferInReturn.ToDictionary(x => x.ProductId, x => x.Qty);
            var sellDict = outboundSell.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferOutDict = transferOut.ToDictionary(x => x.ProductId, x => x.Qty);
            var sampleExportDict = sampleExport.ToDictionary(x => x.ProductId, x => x.Qty);
            var outboundDamageDict = outboundDamage.ToDictionary(x => x.ProductId, x => x.Qty);
            var outboundLostDict = outboundLost.ToDictionary(x => x.ProductId, x => x.Qty);

            // ------------------------ Build list<InventoryReportRow> ------------------------
            var reportData = new List<InventoryReportRow>();

            foreach (var p in products)
            {
                int pid = p.ProductId;
                int beginning = openingStockDict.GetValueOrDefault(pid, 0); int buyQty = buyDict.ContainsKey(pid) ? buyDict[pid] : 0;
                int transNormalQty = transferInNormalDict.ContainsKey(pid) ? transferInNormalDict[pid] : 0;
                int transReturnQty = transferInReturnDict.ContainsKey(pid) ? transferInReturnDict[pid] : 0;
                int sellQty = sellDict.ContainsKey(pid) ? sellDict[pid] : 0;
                int outTransQty = transferOutDict.ContainsKey(pid) ? transferOutDict[pid] : 0;
                int sampleQty = sampleExportDict.ContainsKey(pid) ? sampleExportDict[pid] : 0;
                int outboundDamageQty = outboundDamageDict.ContainsKey(pid) ? outboundDamageDict[pid] : 0;
                int outboundLostQty = outboundLostDict.ContainsKey(pid) ? outboundLostDict[pid] : 0;

                int remain = closingStockDict.GetValueOrDefault(pid, 0);

                reportData.Add(new InventoryReportRow
                {
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    SKU = p.SKU,
                    Beginning = beginning,
                    BuyQty = buyQty,
                    TransferInQty = transNormalQty,
                    ReturnInQty = transReturnQty,
                    SellQty = sellQty,
                    TransferOutQty = outTransQty,
                    SampleExportQty = sampleQty,
                    Remain = remain,
                    OutboundDamage = outboundDamageQty,
                    OutboundLost = outboundLostQty
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

            // Chuyển đổi từ UTC sang giờ địa phương của server
            var serverTimeZone = TimeZoneInfo.Local;

            var startDateUtc = startDate.ToDateTimeUtc();
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, serverTimeZone);
            string startDateStr = startDateLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            var endDateUtc = endDate.ToDateTimeUtc();
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, serverTimeZone);
            string endDateStr = endDateLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

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
                                columns.RelativeColumn(2);  // Chuyển (Nhập)
                                columns.RelativeColumn(1);  // Trả về (Nhập)
                                columns.RelativeColumn(1);  // Hư (Kiểm kê)
                                columns.RelativeColumn(1);  // Mất (Kiểm kê)
                                columns.RelativeColumn(1);  // Bán
                                columns.RelativeColumn(2);  // Chuyển (Xuất)
                                columns.RelativeColumn(1);  // Tồn
                                columns.RelativeColumn(1);  // Xuất mẫu 
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
                                header.Cell().Border(1).AlignCenter().Text("Trả về").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Hư").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Mất").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Bán").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Chuyển\n(Xuất)").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Tồn").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Xuất mẫu").Bold();
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
                                table.Cell().Border(1).AlignRight().Text(r.OutboundDamage.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.OutboundLost.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.SellQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.TransferOutQty.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.Remain.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(r.SampleExportQty.ToString("N0"));
                                stt++;
                            }
                        });
                    });
                });
            }).GeneratePdf();

            // Trả về mảng byte PDF
            return pdfBytes;
        }

        public async Task<byte[]> ExportStockCardPdf(int warehouseId, int productId, Instant startDate, Instant endDate)
        {
            // 1. Tính tồn đầu kỳ: Lấy OpeningStock của Inbound trước startDate
            var openingStockDict = await _unitOfWork.InventoryTransactionRepository
                 .GetAll()
                 .Include(t => t.Lot)
                 .Where(t => t.Lot.WarehouseId == warehouseId
                          && t.CreatedAt <= startDate)
                 .GroupBy(t => t.Lot.ProductId)
                 .Select(g => new
                 {
                     ProductId = g.Key,
                     OpeningStock = g.OrderByDescending(x => x.CreatedAt)
                                     .ThenByDescending(x => x.Id)
                                     .FirstOrDefault().Quantity
                 })
                 .ToDictionaryAsync(x => x.ProductId, x => x.OpeningStock);

            var productEntity = await _unitOfWork.ProductRepository
                 .GetByWhere(p => p.ProductId == productId)
                 .FirstOrDefaultAsync();
            string productName = productEntity?.ProductName ?? productId.ToString();
            string unitType = productEntity?.SKU ?? "";

            // =========================
            // 2. Lấy danh sách Inbound (Nhập mua)
            // =========================
            var inboundList = await _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                    .ThenInclude(i => i.Warehouse)
                .Where(d => d.ProductId == productId
                            && d.Inbound.WarehouseId == warehouseId
                            && d.Inbound.Status == InboundStatus.Completed
                            && d.Inbound.InboundDate >= startDate
                            && d.Inbound.InboundDate <= endDate)
                .GroupBy(d => d.InboundId)
                .Select(g => new
                {
                    InboundId = g.Key,
                    InboundDate = g.First().Inbound.InboundDate,
                    WarehouseDocNumber = g.First().Inbound.Warehouse.DocumentNumber,
                    WarehouseName = g.First().Inbound.Warehouse.WarehouseName,
                    Note = g.First().Inbound.Note ?? "",
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // =========================
            // 3. Lấy danh sách LotTransfer (Nhập chuyển vào)
            // =========================
            var transferInList = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                   .Include(d => d.LotTransfer)
        .ThenInclude(lt => lt.FromWareHouse)
                .Include(d => d.LotTransfer)
                    .ThenInclude(lt => lt.ToWareHouse)
                .Include(d => d.Lot)
                .Where(d => d.Lot.ProductId == productId
                            && d.LotTransfer.ToWareHouseId == warehouseId
                            && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed
                            && d.LotTransfer.CreatedAt >= startDate
                            && d.LotTransfer.CreatedAt <= endDate)
                .GroupBy(d => d.LotTransferId)
                .Select(g => new
                {
                    TransferId = g.Key,
                    TransferDate = g.First().LotTransfer.CreatedAt,
                    WarehouseDocNumber = g.First().LotTransfer.ToWareHouse.DocumentNumber,
                    WarehouseName = g.First().LotTransfer.ToWareHouse.WarehouseName,
                    Note = $"Chuyển từ kho {g.First().LotTransfer.FromWareHouse.WarehouseName} sang kho {g.First().LotTransfer.ToWareHouse.WarehouseName}",
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // =========================
            // 4.1. Lấy danh sách Xuất Mất (Hàng tìm thấy)
            // =========================
            var outboundFound = await _unitOfWork.InventoryCheckRepository
                .GetAll()
                .Include(ic => ic.InventoryCheckDetails)
                    .ThenInclude(icd => icd.Lot)
                .Where(ic => ic.WarehouseId == warehouseId
                    && ic.CheckDate >= startDate
                    && ic.CheckDate <= endDate)
                .SelectMany(ic => ic.InventoryCheckDetails)
                .Where(icd => icd.Lot.ProductId == productId
                    && icd.Status == InventoryCheckStatus.Found && icd.Quantity > 0)
                .GroupBy(icd => icd.InventoryCheckId)
                .Select(g => new
                {
                    InventoryCheckId = g.Key,
                    CheckDate = g.First().InventoryCheck.CheckDate,
                    CustomerDocNumber = g.First().InventoryCheck.Warehouse.DocumentNumber ?? "",
                    CustomerName = g.First().InventoryCheck.Warehouse.WarehouseName,
                    Note = "Hàng mất đã được tìm thấy",
                    Qty = g.Sum(icd => icd.Quantity)
                })
                .ToListAsync();

            // Các giao dịch nhập (mỗi giao dịch ở 1 dòng riêng)
            var inboundTransactions = new List<StockCardLine>();
            foreach (var i in inboundList)
            {
                inboundTransactions.Add(new StockCardLine
                {
                    Date = i.InboundDate?.ToDateTimeUtc() ?? DateTime.MinValue,
                    DocumentNumber = i.WarehouseDocNumber ?? "",
                    PartnerName = i.WarehouseName,
                    Note = i.Note,
                    InQty = i.Qty,
                    OutQty = 0
                });
            }
            foreach (var t in transferInList)
            {
                inboundTransactions.Add(new StockCardLine
                {
                    Date = t.TransferDate.ToDateTimeUtc(),
                    DocumentNumber = t.WarehouseDocNumber ?? "",
                    PartnerName = t.WarehouseName,
                    Note = t.Note,
                    InQty = t.Qty,
                    OutQty = 0
                });
            }
            foreach (var t in outboundFound)
            {
                inboundTransactions.Add(new StockCardLine
                {
                    Date = t.CheckDate.ToDateTimeUtc(),
                    DocumentNumber = t.CustomerDocNumber ?? "",
                    PartnerName = "Kiểm kho",
                    Note = t.Note,
                    InQty = t.Qty,
                    OutQty = 0
                });
            }

            // =========================
            // 4.2. Lấy danh sách Xuất Mất (Hàng mất)
            // =========================
            var outboundLost = await _unitOfWork.InventoryCheckRepository
                .GetAll()
                .Include(ic => ic.InventoryCheckDetails)
                    .ThenInclude(icd => icd.Lot)
                .Where(ic => ic.WarehouseId == warehouseId
                    && ic.CheckDate >= startDate
                    && ic.CheckDate <= endDate)
                .SelectMany(ic => ic.InventoryCheckDetails)
                .Where(icd => icd.Lot.ProductId == productId
                    && icd.Status == InventoryCheckStatus.Lost && icd.Quantity > 0)
                .GroupBy(icd => icd.InventoryCheckId)
                .Select(g => new
                {
                    InventoryCheckId = g.Key,
                    CheckDate = g.First().InventoryCheck.CheckDate,
                    CustomerDocNumber = g.First().InventoryCheck.Warehouse.DocumentNumber ?? "",
                    CustomerName = g.First().InventoryCheck.Warehouse.WarehouseName,
                    Note = "Hàng mất từ kiểm kho",
                    Qty = g.Sum(icd => icd.Quantity)
                })
                .ToListAsync();

            // 5. Lấy danh sách Outbound (Xuất bán)
            // Bao gồm cả các Outbound có trạng thái Completed hoặc Returned để đảm bảo nếu Outbound bị update thành Returned thì vẫn được tính vào "Xuất trong kỳ"
            var outboundList = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(d => d.Outbound)
                    .ThenInclude(o => o.Customer)
                .Include(d => d.Lot)
                .Where(d => d.Lot.ProductId == productId
                            && d.Lot.WarehouseId == warehouseId
                            && (d.Outbound.Status == OutboundStatus.Completed || d.Outbound.Status == OutboundStatus.Returned)
                            && d.Outbound.OutboundDate >= startDate
                            && d.Outbound.OutboundDate <= endDate)
                .GroupBy(d => d.OutboundId)
                .Select(g => new
                {
                    OutboundId = g.Key,
                    OutboundDate = g.First().Outbound.OutboundDate,
                    CustomerDocNumber = g.First().Outbound.Customer.DocumentNumber,
                    CustomerName = g.First().Outbound.Customer.CustomerName,
                    Note = g.First().Outbound.Note ?? "",
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            // 6. Lấy danh sách LotTransfer (Xuất chuyển ra)
            var transferOutList = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                        .ThenInclude(lt => lt.FromWareHouse)
                .Include(d => d.LotTransfer)
                        .ThenInclude(lt => lt.ToWareHouse)
                .Include(d => d.Lot)
                .Where(d => d.Lot.ProductId == productId
                            && d.LotTransfer.FromWareHouseId == warehouseId
                            && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed
                            && d.LotTransfer.CreatedAt >= startDate
                            && d.LotTransfer.CreatedAt <= endDate)
                .GroupBy(d => d.LotTransferId)
                .Select(g => new
                {
                    TransferId = g.Key,
                    TransferDate = g.First().LotTransfer.CreatedAt,
                    CustomerDocNumber = g.First().LotTransfer.FromWareHouse.DocumentNumber,
                    CustomerName = g.First().LotTransfer.FromWareHouse.WarehouseName,
                    Note = $"Chuyển từ  {g.First().LotTransfer.FromWareHouse.WarehouseName} sang  {g.First().LotTransfer.ToWareHouse.WarehouseName}",
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();

            var outboundTransactions = new List<StockCardLine>();
            foreach (var o in outboundList)
            {
                outboundTransactions.Add(new StockCardLine
                {
                    Date = o.OutboundDate.HasValue ? o.OutboundDate.Value.ToDateTimeUtc() : DateTime.MinValue,
                    DocumentNumber = o.CustomerDocNumber ?? "",
                    PartnerName = o.CustomerName,
                    Note = o.Note,
                    InQty = 0,
                    OutQty = o.Qty
                });
            }
            foreach (var t in transferOutList)
            {
                outboundTransactions.Add(new StockCardLine
                {
                    Date = t.TransferDate.ToDateTimeUtc(),
                    DocumentNumber = t.CustomerDocNumber ?? "",
                    PartnerName = t.CustomerName,
                    Note = t.Note,
                    InQty = 0,
                    OutQty = t.Qty
                });
            }
            foreach (var lost in outboundLost)
            {
                outboundTransactions.Add(new StockCardLine
                {
                    Date = lost.CheckDate.ToDateTimeUtc(),
                    DocumentNumber = lost.CustomerDocNumber ?? "",
                    PartnerName = "Kiểm kho",
                    Note = lost.Note,
                    InQty = 0,
                    OutQty = lost.Qty
                });
            }
            var closingStockDict = await _unitOfWork.InventoryTransactionRepository
                .GetAll()
                .Include(t => t.Lot)
                .Where(t => t.Lot.WarehouseId == warehouseId
                         && t.CreatedAt <= endDate)
                .GroupBy(t => t.Lot.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ClosingStock = g.OrderByDescending(x => x.CreatedAt)
                                    .ThenByDescending(x => x.Id)
                                    .FirstOrDefault().Quantity
                })
                .ToDictionaryAsync(x => x.ProductId, x => x.ClosingStock);
            int beginningBalance = openingStockDict.GetValueOrDefault(productId, 0);
            int endingBalance = closingStockDict.GetValueOrDefault(productId, beginningBalance);
            // 6. Gộp tất cả giao dịch và sắp xếp theo ngày 
            var allTransactions = inboundTransactions
                .Concat(outboundTransactions)
                .OrderBy(t => t.Date)
                .ToList();

            var stockCardLines = new List<StockCardDto>();
            int running = beginningBalance;
            foreach (var t in allTransactions)
            {
                var beginBal = running;
                var endBal = beginBal + t.InQty - t.OutQty;
                stockCardLines.Add(new StockCardDto
                {
                    TransactionDate = t.Date,
                    DocumentNumber = t.DocumentNumber,
                    PartnerName = t.PartnerName,
                    Note = t.Note,
                    QuantityIn = t.InQty,
                    QuantityOut = t.OutQty,
                    BeginningBalance = beginBal,
                    EndingBalance = endBal
                });
                running = endBal;
            }

            // =========================
            // 9. Xuất PDF bằng QuestPDF
            // =========================
            var warehouseEntity = await _unitOfWork.WarehouseRepository
                .GetByWhere(w => w.WarehouseId == warehouseId)
                .FirstOrDefaultAsync();
            string warehouseNameForCard = warehouseEntity?.WarehouseName ?? "N/A";
            var serverTimeZone = TimeZoneInfo.Local;
            var startDateUtc = startDate.ToDateTimeUtc();
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDateUtc, serverTimeZone);
            string startDateStrCard = startDateLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var endDateUtc = endDate.ToDateTimeUtc();
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDateUtc, serverTimeZone);
            string endDateStrCard = endDateLocal.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);


            Settings.License = LicenseType.Community;
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("CÔNG TY TNHH DƯỢC PHẨM TRUNG HẠNH").Bold().FontSize(12).AlignCenter();
                        col.Item().Text("THẺ KHO").Bold().FontSize(14).AlignCenter();
                        col.Item().Text($"Kho: {warehouseNameForCard}").AlignCenter();
                        col.Item().Text($"Từ ngày {startDateStrCard} đến ngày {endDateStrCard}").AlignCenter();
                        col.Item().Text($"Sản phẩm: {productName}").AlignCenter();
                        col.Item().Text($"Đơn vị tính: {unitType}").AlignCenter();
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25); // STT
                                columns.RelativeColumn(2);  // Chứng từ số
                                columns.RelativeColumn(2);  // Ngày
                                columns.RelativeColumn(3);  // Đối tác
                                columns.RelativeColumn(2);  // Đầu kỳ
                                columns.RelativeColumn(2);  // Nhập trong kỳ
                                columns.RelativeColumn(2);  // Xuất trong kỳ
                                columns.RelativeColumn(2);  // Tồn cuối
                                columns.RelativeColumn(3);  // Ghi chú
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).AlignCenter().Text("STT").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Chứng từ số").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Ngày").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Đối tác").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Đầu kỳ").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Nhập trong kỳ").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Xuất trong kỳ").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Tồn cuối").Bold();
                                header.Cell().Border(1).AlignCenter().Text("Ghi chú").Bold();
                            });

                            int idx = 1;
                            foreach (var line in stockCardLines)
                            {
                                table.Cell().Border(1).AlignCenter().Text(idx.ToString());
                                table.Cell().Border(1).Text(line.DocumentNumber);
                                table.Cell().Border(1).AlignCenter().Text(line.TransactionDate == DateTime.MinValue ? "" : line.TransactionDate.ToString("dd/MM/yyyy"));
                                table.Cell().Border(1).Text(line.PartnerName);
                                table.Cell().Border(1).AlignRight().Text(line.BeginningBalance.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(line.QuantityIn.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(line.QuantityOut.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(line.EndingBalance.ToString("N0"));
                                table.Cell().Border(1).Text(line.Note);
                                idx++;
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return pdfBytes;
        }
    }
}
