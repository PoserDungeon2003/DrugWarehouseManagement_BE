using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class InventoryReportService : IInventoryReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public byte[] ExportInventoryReport(int warehouseId, Instant startDate, Instant endDate)
        {
            // 1) Lấy danh sách Product đang có trong kho
            //    Thông qua Lot (warehouseId)
            var productsQuery = _unitOfWork.LotRepository
                .GetAll()
                .Where(l => l.WarehouseId == warehouseId)
                .Select(l => l.Product)
                .Distinct();

            var products = productsQuery.ToList();

            // 2) Lấy Đầu kỳ (OpeningStock) từ inbound của ngày gần nhất (< startDate)
            //    Chỉ lấy các inbound có tồn tại inboundrequest (InboundRequestId != null)
            var openingStockDict = _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                .Where(d => d.Inbound.WarehouseId == warehouseId
                            && d.Inbound.InboundDate < startDate
                            && d.Inbound.Status != InboundStatus.Cancelled
                            && d.Inbound.InboundRequestId != null)
                .GroupBy(d => d.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    // Get the OpeningStock from the inbound detail with the maximum inbound date.
                    OpeningStock = g.OrderByDescending(x => x.Inbound.InboundDate)
                                    .FirstOrDefault().OpeningStock
                })
                .ToDictionary(x => x.ProductId, x => x.OpeningStock);

            // 3) Tính Nhập (Mua) trong [startDate, endDate]
            var inboundBuy = _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                .Where(d => d.Inbound.WarehouseId == warehouseId
                         && d.Inbound.InboundDate >= startDate
                         && d.Inbound.InboundDate <= endDate
                         && d.Inbound.Status == InboundStatus.Completed
                         && d.Inbound.InboundRequestId != null)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();
            var transferIn = _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                .Where(d => d.LotTransfer.ToWareHouseId == warehouseId
                            && d.LotTransfer.CreatedAt >= startDate
                            && d.LotTransfer.CreatedAt <= endDate
                            && d.LotTransfer.LotTransferStatus == LotTransferStatus.Completed)
                .GroupBy(d => d.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();

            // 4) Tính Xuất (Bán) - OutboundDetails -> Outbound => Status = Completed
            var outboundSell = _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Include(od => od.Lot)
                .Where(od => od.Lot.WarehouseId == warehouseId
                             && od.Outbound.OutboundDate >= startDate
                             && od.Outbound.OutboundDate <= endDate
                             && od.Outbound.Status == OutboundStatus.Completed)
                .GroupBy(od => od.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();

            // 5) Tính "Khác" - các đơn trả về (OutboundStatus == Returned)
            var outboundReturned = _unitOfWork.OutboundDetailsRepository
                .GetAll()
                .Include(od => od.Outbound)
                .Include(od => od.Lot)
                .Where(od => od.Lot.WarehouseId == warehouseId
                             && od.Outbound.OutboundDate >= startDate
                             && od.Outbound.OutboundDate <= endDate
                             && od.Outbound.Status == OutboundStatus.Returned)
                .GroupBy(od => od.Lot.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();

            // Tạo dictionary
            var buyDict = inboundBuy.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferInDict = transferIn.ToDictionary(x => x.ProductId, x => x.Qty);
            var sellDict = outboundSell.ToDictionary(x => x.ProductId, x => x.Qty);
            var returnedDict = outboundReturned.ToDictionary(x => x.ProductId, x => x.Qty);
            // 6) Tạo file Excel
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("InventoryReport");

            // Header
            ws.Cells[1, 1].Value = "Mã hàng";
            ws.Cells[1, 2].Value = "Tên hàng";
            ws.Cells[1, 3].Value = "Đơn vị tính";
            ws.Cells[1, 4].Value = "Đầu kỳ";
            ws.Cells[1, 5].Value = "Mua";
            ws.Cells[1, 6].Value = "Chuyển";
            ws.Cells[1, 7].Value = "Bán";
            ws.Cells[1, 8].Value = "Khác";
            ws.Cells[1, 9].Value = "Tồn";

            int row = 2;
            foreach (var p in products)
            {
                int pid = p.ProductId;

                // Đầu kỳ
                int beginning = openingStockDict.ContainsKey(pid) ? (openingStockDict[pid] ?? 0) : 0;

                // Mua
                int buyQty = buyDict.ContainsKey(pid) ? buyDict[pid] : 0;

                // Chuyển
                int transferInQty = transferInDict.ContainsKey(pid) ? transferInDict[pid] : 0;

                // Khác - Đơn trả về
                int returnedQty = returnedDict.ContainsKey(pid) ? returnedDict[pid] : 0;

                // Bán
                int sellQty = sellDict.ContainsKey(pid) ? sellDict[pid] : 0;

                // Tồn = Đầu kỳ + (Mua + Chuyển) - (Bán + khác)
                int remain = beginning + buyQty + transferInQty - sellQty - returnedQty;
                ws.Cells[row, 1].Value = p.ProductCode;
                ws.Cells[row, 2].Value = p.ProductName;
                ws.Cells[row, 3].Value = p.SKU; // Tùy logic
                ws.Cells[row, 4].Value = beginning;
                ws.Cells[row, 5].Value = buyQty;
                ws.Cells[row, 6].Value = transferInQty;
                ws.Cells[row, 7].Value = sellQty;
                ws.Cells[row, 8].Value = returnedQty;
                ws.Cells[row, 9].Value = remain;

                row++;
            }
            ws.Cells[1, 1, row, 8].AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
