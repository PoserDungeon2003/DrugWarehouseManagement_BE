using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
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
    public class InventoryReportService
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

            // 2) Lấy Đầu kỳ (OpeningStock) chỉ cho các inbound < startDate
            //    => Tồn đầu kỳ
            var openingStockDict = _unitOfWork.InboundDetailRepository
                .GetAll()
                .Include(d => d.Inbound)
                .Where(d => d.Inbound.WarehouseId == warehouseId
                            && d.Inbound.InboundDate < startDate
                            && d.Inbound.Status != InboundStatus.Cancelled)
                .GroupBy(d => d.Inbound.ProductId)
                .Select(g => new { ProductId = g.Key, OpeningStock = g.Sum(x => x.OpeningStock) })
                .ToDictionary(x => x.ProductId, x => x.OpeningStock);

            // 3) Tính Nhập (Mua) trong [startDate, endDate]
            var inboundBuy = _unitOfWork.InboundRepository
                .GetAll()
                .Where(i => i.WarehouseId == warehouseId
                            && i.InboundDate >= startDate
                            && i.InboundDate <= endDate
                            && i.Status != InboundStatus.Cancelled)
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();
            var transferIn = _unitOfWork.LotTransferDetailsRepository
                .GetAll()
                .Include(d => d.LotTransfer)
                .Where(d => d.LotTransfer.ToWareHouseId == warehouseId
                            && d.LotTransfer.CreatedAt >= startDate
                            && d.LotTransfer.CreatedAt <= endDate
                            && d.LotTransfer.LotTransferStatus != LotTransferStatus.Cancelled)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToList();

            // 5) Tính Xuất (Bán) - OutboundDetails -> Outbound => Status = Completed
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

            // Tạo dictionary
            var buyDict = inboundBuy.ToDictionary(x => x.ProductId, x => x.Qty);
            var transferInDict = transferIn.ToDictionary(x => x.ProductId, x => x.Qty);
            var sellDict = outboundSell.ToDictionary(x => x.ProductId, x => x.Qty);

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
            ws.Cells[1, 8].Value = "Tồn";

            int row = 2;
            foreach (var p in products)
            {
                int pid = p.ProductId;

                // Đầu kỳ
                int beginning = openingStockDict.ContainsKey(pid) ? openingStockDict[pid] : 0;

                // Mua
                int buyQty = buyDict.ContainsKey(pid) ? buyDict[pid] : 0;

                // Chuyển
                int transferInQty = transferInDict.ContainsKey(pid) ? transferInDict[pid] : 0;

                // Bán
                int sellQty = sellDict.ContainsKey(pid) ? sellDict[pid] : 0;

                // Tồn = Đầu kỳ + (Mua + Chuyển) - Bán
                int remain = beginning + buyQty + transferInQty - sellQty;
                ws.Cells[row, 1].Value = p.ProductCode;
                ws.Cells[row, 2].Value = p.ProductName;
                ws.Cells[row, 3].Value = "Hộp"; // Tùy logic
                ws.Cells[row, 4].Value = beginning;
                ws.Cells[row, 5].Value = buyQty;
                ws.Cells[row, 6].Value = transferInQty;
                ws.Cells[row, 7].Value = sellQty;
                ws.Cells[row, 8].Value = remain;

                row++;
            }
            ws.Cells[1, 1, row, 8].AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}
