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
                .Include(d => d.Lot) 
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
                             && od.TotalPrice > 0
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

            // (d3) Xuất mẫu (Outbound có TotalPrice = 0)
            var sampleExport = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Include(od => od.Lot)
                .Where(od => od.Lot.WarehouseId == warehouseId
                             && od.Outbound.OutboundDate >= startDate
                             && od.Outbound.OutboundDate <= endDate
                             && od.Outbound.Status == OutboundStatus.Completed
                             && od.TotalPrice == 0) // Điều kiện xuất mẫu
                .GroupBy(od => od.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToListAsync();


            // ------------------------ Tạo dictionary ------------------------

            var buyDict = inboundBuy.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferInDict = transferIn.ToDictionary(x => x.ProductId, x => x.Qty);
            var inboundReturnDict = inboundReturn.ToDictionary(x => x.ProductId, x => x.Qty);
            var sellDict = outboundSell.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferOutDict = transferOut.ToDictionary(x => x.ProductId, x => x.Qty);
            var sampleExportDict = sampleExport.ToDictionary(x => x.ProductId, x => x.Qty);

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
                int sampleQty = sampleExportDict.ContainsKey(pid) ? sampleExportDict[pid] : 0;
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
                    SampleExportQty = sampleQty,
                    Remain = remain - sampleQty
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
                                columns.RelativeColumn(1); // xuất mẫu 
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
            // 1. Tính tồn đầu kỳ          
            // Lấy OpeningStock từ InboundDetails mới nhất (trước startDate)
            var beginningBalance = await _unitOfWork.InboundDetailRepository
                 .GetAll()
                 .Include(d => d.Inbound)
                 .Where(d => d.ProductId == productId
                             && d.Inbound.WarehouseId == warehouseId
                             && d.Inbound.Status == InboundStatus.Completed
                             && d.Inbound.InboundDate < startDate)
                 .OrderByDescending(d => d.Inbound.InboundDate)
                 .Select(d => d.OpeningStock)
                 .FirstOrDefaultAsync() ?? 0;

            // =========================
            // 2. Lấy danh sách Inbound (Nhập)
            // =========================
            // Gồm: Nhập mua (InboundRequestId != null), Nhập trả về (InboundRequestId == null)
            // Mỗi phiếu inbound -> 1 dòng. DocumentNumber = Warehouse.DocumentNumber, Tên khách hàng = WarehouseName
            // Số lượng = tổng quantity InboundDetails => ta gộp theo InboundId
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
                    // Mỗi inboundId -> 1 record
                    InboundId = g.Key,
                    InboundDate = g.First().Inbound.InboundDate,  // Tất cả inboundDetail chung 1 inboundDate
                    WarehouseDocNumber = g.First().Inbound.Warehouse.DocumentNumber, // Số chứng từ -> Warehouse
                    WarehouseName = g.First().Inbound.Warehouse.WarehouseName,       // Tên khách hàng => Tên kho
                    Note = g.First().Inbound.Note ?? "",
                    Qty = g.Sum(x => x.Quantity) // Tổng quantity
                })
                .ToListAsync();

            // =========================
            // 3. Lấy danh sách LotTransfer (Nhập chuyển vào)
            // =========================
            // Mỗi lot transfer -> 1 dòng inbound
            var transferInList = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
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
                    // Mỗi lotTransfer -> 1 record
                    TransferId = g.Key,
                    TransferDate = g.First().LotTransfer.CreatedAt,
                    WarehouseDocNumber = g.First().LotTransfer.ToWareHouse.DocumentNumber, // Số chứng từ -> toWarehouse
                    WarehouseName = g.First().LotTransfer.ToWareHouse.WarehouseName,
                    Note = "Chuyển kho vào",
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();



            // Gộp inbound + transferIn => inboundTransactions
            var inboundTransactions = new List<StockCardLine>();
            // Chuyển inbound -> StockCardLine
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
            // Chuyển transferIn -> StockCardLine
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

          // =========================
            // 4. Lấy danh sách Outbound (Xuất)
            // =========================
            // Gồm: Xuất bán, Xuất mẫu (TotalPrice=0), Mỗi Outbound => 1 dòng
            var outboundList = await _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(d => d.Outbound)
                    .ThenInclude(o => o.Customer)
                .Include(d => d.Lot)
                .Where(d => d.Lot.ProductId == productId
                            && d.Outbound.Status == OutboundStatus.Completed
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

            // =========================
            // 5. Lấy danh sách LotTransfer (Xuất chuyển ra)
            // =========================
            // Mỗi lot transfer -> 1 dòng outbound
            var transferOutList = await _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                    .ThenInclude(lt => lt.FromWareHouse)
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
                    Note = "Chuyển kho ra",
                    Qty = g.Sum(x => x.Quantity)
                })
                .ToListAsync();
            // Gộp outbound + transferOut => outboundTransactions
            var outboundTransactions = new List<StockCardLine>();
            // Chuyển outbound -> StockCardLine
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
            // Chuyển transferOut -> StockCardLine
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

            // =========================
            // 6. Gộp inboundTransactions + outboundTransactions
            // =========================
            var allTransactions = inboundTransactions
                .Concat(outboundTransactions)
                .OrderBy(t => t.Date)
                .ToList();

            // =========================
            // 7. Tính luỹ kế: Tồn cuối
            // =========================
            var stockCardLines = new List<StockCardDto>();

            // Dòng đầu tiên: tồn đầu kỳ
            stockCardLines.Add(new StockCardDto
            {
                TransactionDate = DateTime.MinValue,
                DocumentNumber = "",
                PartnerName = "",
                Note = "Tồn đầu kỳ",
                QuantityIn = 0,
                QuantityOut = 0,
                BeginningBalance = beginningBalance,
                EndingBalance = beginningBalance
            });

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
            // 8. Xuất PDF bằng QuestPDF
            // =========================
            // Lấy thông tin kho, format ngày
            var warehouseEntity = await _unitOfWork.WarehouseRepository
                .GetByWhere(w => w.WarehouseId == warehouseId)
                .FirstOrDefaultAsync();
            string warehouseName = warehouseEntity?.WarehouseName ?? "N/A";

            string startDateStr = startDate.ToDateTimeUtc().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            string endDateStr = endDate.ToDateTimeUtc().ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            Settings.License = LicenseType.Community;
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Item().Text("CÔNG TY TNHH DƯỢC PHẨM TRUNG HẠNH").Bold().FontSize(12).AlignCenter();
                        col.Item().Text("THẺ KHO").Bold().FontSize(14).AlignCenter();
                        col.Item().Text($"Kho: {warehouseName}").AlignCenter();
                        col.Item().Text($"Từ ngày {startDateStr} đến ngày {endDateStr}").AlignCenter();
                        col.Item().Text($"Mã SP: {productId}").AlignCenter(); // Hoặc tên SP
                    });

                    // Content
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

                            // Header
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
                                table.Cell().Border(1).AlignCenter().Text(idx);
                                table.Cell().Border(1).Text(line.DocumentNumber);
                                // Nếu TransactionDate == DateTime.MinValue => dòng tồn đầu kỳ => rỗng
                                table.Cell().Border(1).AlignCenter().Text(
                                    line.TransactionDate == DateTime.MinValue
                                        ? ""
                                        : line.TransactionDate.ToString("dd/MM/yyyy"));
                                table.Cell().Border(1).Text(line.PartnerName);

                                table.Cell().Border(1).AlignRight().Text(line.QuantityIn.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(line.QuantityOut.ToString("N0"));
                                table.Cell().Border(1).AlignRight().Text(line.EndingBalance.ToString("N0"));
                                table.Cell().Border(1).Text(line.Note);

                                idx++;
                            }
                        });
                    });

                    // Footer (ký tên)
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Người lập\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Thủ kho\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Kế toán\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Giám đốc\n\n(Ký, họ tên)").AlignCenter();
                    });
                });
            }).GeneratePdf();

            return pdfBytes;
        }
    }
}

