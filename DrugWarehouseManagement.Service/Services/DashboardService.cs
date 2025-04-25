using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DrugWarehouseManagement.Service.DTO.Response.DashboardReportDto;

namespace DrugWarehouseManagement.Service.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<DashboardReportDto> GetDashboardReportAsync(string role, TimeFilterOption filterOption)
        {
            var (startInstant, endInstant) = DateTimeHelper.GetInstantRange(filterOption);
            var dashboard = new DashboardReportDto();
            // --- Tổng số liệu đơn hàng ---
            //lấy số đơn được tạo trong kỳ
            var totalOrdersCreated = await _unitOfWork.OutboundRepository
                 .GetAll()
                 .Where(o =>
                    o.CreatedAt >= startInstant &&
                    o.CreatedAt <= endInstant)
                 .CountAsync();
            dashboard.TotalOutboundOrders = totalOrdersCreated;
            //số đơn nhập được tạo trong kỳ
            var inboundList = await _unitOfWork.InboundRepository
                  .GetAll()
                  .Where(i =>
                      i.CreatedAt >= startInstant &&
                      i.CreatedAt <= endInstant
                  )
                  .CountAsync();
            //số đơn chuyển kho được tạo trong kỳ
            var lotTransferList = await _unitOfWork.LotTransferRepository
                  .GetAll()
                  .Where(l =>
                      l.CreatedAt >= startInstant &&
                      l.CreatedAt <= endInstant &&
                      l.LotTransferStatus == LotTransferStatus.Completed
                  )
                  .CountAsync();
            dashboard.TotalOutboundOrders = totalOrdersCreated;
            dashboard.TotalInboundOrders = inboundList;
            dashboard.TotalLotTransferOrders = lotTransferList;

            // --- Đơn xuất theo trạng thái ---
            dashboard.OutboundCompletedCount = await _unitOfWork.OutboundRepository
                   .GetAll()
                   .CountAsync(o =>
                               o.Status == OutboundStatus.Completed &&
                               o.OutboundDate >= startInstant &&
                               o.OutboundDate <= endInstant);

            dashboard.OutboundCancelledCount = await _unitOfWork.OutboundRepository
                    .GetAll()
                    .CountAsync(o =>
                                o.Status == OutboundStatus.Cancelled &&
                                o.UpdatedAt >= startInstant &&
                                o.UpdatedAt <= endInstant);
            dashboard.OutboundReturnedCount = await _unitOfWork.OutboundRepository
                    .GetAll()
                    .CountAsync(o =>
                                o.Status == OutboundStatus.Returned &&
                                o.OutboundDate >= startInstant &&
                                o.OutboundDate <= endInstant);

            dashboard.OutboundSampleCount = await _unitOfWork.OutboundRepository
                     .GetAll()
                     .Where(o =>
                          o.Status == OutboundStatus.Completed &&
                          o.OutboundDate >= startInstant &&
                          o.OutboundDate <= endInstant)
                     .SelectMany(o => o.OutboundDetails)
                     .Where(od => od.TotalPrice == 0)
                     .Select(od => od.OutboundId)
                     .Distinct()
                     .CountAsync();
            // --- Sản phẩm tồn kho nhiều nhất ---
            var bestStockedProductQuery = await _unitOfWork.LotRepository
                    .GetAll()
                    .GroupBy(l => l.ProductId)
                    .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(x => x.Quantity) })
                    .OrderByDescending(x => x.TotalStock)
                    .FirstOrDefaultAsync();
            if (bestStockedProductQuery != null)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(bestStockedProductQuery.ProductId);
                dashboard.BestStockedProduct = new ProductStatisticDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    TotalQuantity = bestStockedProductQuery.TotalStock
                };
            }
            // --- Sản phẩm được xuất/nhập nhiều nhất ---
            var bestExportQuery = await _unitOfWork.OutboundDetailsRepository
            .GetAll()
            .Include(od => od.Lot)
            .Where(od => od.Outbound.OutboundDate >= startInstant && od.Outbound.OutboundDate <= endInstant)
            .GroupBy(od => od.Lot.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.TotalQty)
            .FirstOrDefaultAsync();

            if (bestExportQuery != null)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(bestExportQuery.ProductId);
                dashboard.BestExportedProduct = new ProductStatisticDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    TotalQuantity = bestExportQuery.TotalQty
                };
            }
            else
            {
                dashboard.BestExportedProduct = new ProductStatisticDto();
            }

            var bestImportQuery = await _unitOfWork.InboundDetailRepository
          .GetAll()
          .Where(d => d.Inbound.InboundDate >= startInstant && d.Inbound.InboundDate <= endInstant)
          .GroupBy(d => d.ProductId)
          .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
          .OrderByDescending(x => x.TotalQty)
          .FirstOrDefaultAsync();

            if (bestImportQuery != null)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(bestImportQuery.ProductId);
                dashboard.BestImportedProduct = new ProductStatisticDto
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    TotalQuantity = bestImportQuery.TotalQty
                };
            }
            else
            {
                dashboard.BestImportedProduct = new ProductStatisticDto();
            }
            // 1. Phân loại đơn nhập:
            // - Đơn trả về (InboundRequestId == null)
            dashboard.InboundClassification = new InboundClassificationDto();
            dashboard.InboundClassification.InboundReturnCount =
                await _unitOfWork.InboundRepository.GetAll()
                  .Where(i =>
                      i.InboundDate >= startInstant &&
                      i.InboundDate <= endInstant &&
                      i.Status == InboundStatus.Completed &&
                      i.InboundRequestId == null)
                  .CountAsync();

            // - Đơn nhập bình thường (InboundRequestId != null)
            dashboard.InboundClassification.InboundNormalCount =
                await _unitOfWork.InboundRepository.GetAll()
                  .Where(i =>
                      i.InboundDate >= startInstant &&
                      i.InboundDate <= endInstant &&
                      i.Status == InboundStatus.Completed &&
                      i.InboundRequestId != null)
                  .CountAsync();

            // 2. Tổng giá trị nhập:
            dashboard.TotalInboundValue =
                await _unitOfWork.InboundDetailRepository.GetAll()
                  .Where(d =>
                      d.Inbound.InboundDate >= startInstant &&
                      d.Inbound.InboundDate <= endInstant &&
                      d.Inbound.Status == InboundStatus.Completed)
                  .SumAsync(d => d.TotalPrice);
            // 3. Tổng giá trị xuất:
            dashboard.TotalOutboundValue =
                await _unitOfWork.OutboundDetailsRepository.GetAll()
                  .Where(d =>
                      d.Outbound.OutboundDate >= startInstant &&
                      d.Outbound.OutboundDate <= endInstant &&
                      (d.Outbound.Status == OutboundStatus.Completed ||
                       d.Outbound.Status == OutboundStatus.Returned))
                  .SumAsync(d => d.TotalPrice);
            // --- Danh sách chứng từ mới tạo ---

            var inboundReports = await _unitOfWork.InboundReportRepository
                .GetAll()
                .Include(r => r.Inbound)
                .Where(r =>
                    r.ReportDate >= startInstant &&
                    r.ReportDate <= endInstant
                )
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            dashboard.NewDocuments = inboundReports
                .Select(r => new DocumentStatusDto
                {
                    DocumentId = r.InboundReportId,
                    DocumentType = "InboundReport",
                    DocumentCode = r.Inbound.InboundCode ?? $"IR-{r.InboundReportId}",
                    Status = r.Status.ToString(),
                    CreatedAt = r.CreatedAt.ToDateTimeUtc()
                })
                .ToList();
            // --- Danh sách sản phẩm dưới mức quy định hoặc sắp hết hàng ---
            int lowStockThreshold = 30;

            // 1. Sum quantities per product, excluding warehouse 2 and 6
            var lowStockQuery = await _unitOfWork.LotRepository
                .GetAll()
                .Where(l => l.Warehouse.WarehouseId != 6
                         && l.Warehouse.WarehouseId != 2)
                .GroupBy(l => l.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    TotalStock = g.Sum(x => x.Quantity)
                })
                // 2. Keep only those below threshold
                .Where(x => x.TotalStock < lowStockThreshold)
                // 3. Join back to Products to get name, code, etc.
                .Join(_unitOfWork.ProductRepository.GetAll(),
                      stock => stock.ProductId,
                      prod => prod.ProductId,
                      (stock, prod) => new ProductLowStockDto
                      {
                          ProductId = prod.ProductId,
                          ProductName = prod.ProductName,
                          CurrentStock = stock.TotalStock,
                          Threshold = lowStockThreshold
                      })
                .ToListAsync();

            dashboard.LowStockProducts = lowStockQuery;

            // --- Danh sách đơn hàng ---
            DateTime now = DateTime.UtcNow;
            var newOrders = await _unitOfWork.OutboundRepository
                .GetAll()
                .Where(o =>
                     o.CreatedAt >= startInstant &&
                     o.CreatedAt <= endInstant &&
                     o.Status == OutboundStatus.Pending)
                 .Select(o => new DashboardReportDto.OrderDto
                 {
                     OrderId = o.OutboundId,
                     OrderCode = o.OutboundCode,
                     Status = o.Status.ToString(),
                     CreatedAt = o.CreatedAt.ToDateTimeUtc()  // still UTC timestamp
                 })
                     .ToListAsync();

            var processingOrders = await _unitOfWork.OutboundRepository
                        .GetAll()
                        .Where(o =>
                            o.CreatedAt >= startInstant &&
                            o.CreatedAt <= endInstant &&
                            o.Status == OutboundStatus.InProgress)
                        .Select(o => new DashboardReportDto.OrderDto
                        {
                            OrderId = o.OutboundId,
                            OrderCode = o.OutboundCode,
                            Status = o.Status.ToString(),
                            CreatedAt = o.CreatedAt.ToDateTimeUtc()
                        })
                        .ToListAsync();


            dashboard.OrderSummary = new OrderSummaryDto
            {
                NewOrders = newOrders,
                ProcessingOrders = processingOrders
            }; ;

            //danh sach inbound dang pending 

            dashboard.NewInboundOrders = await _unitOfWork.InboundRepository
                .GetAll()
                .Where(o =>
                    o.CreatedAt >= startInstant &&
                    o.CreatedAt <= endInstant &&
                    o.Status == InboundStatus.Pending)
                .Select(o => new DashboardReportDto.OrderDto
                {
                    OrderId = o.InboundId,
                    OrderCode = o.InboundCode,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt.ToDateTimeUtc()
                })
                .ToListAsync();

            dashboard.AccountantInboundOrders = await _unitOfWork.InboundRequestRepository
               .GetAll()
               .Where(o =>
                   o.CreatedAt >= startInstant &&
                   o.CreatedAt <= endInstant &&
                   o.Status == InboundRequestStatus.WaitingForAccountantApproval)
               .Select(o => new DashboardReportDto.OrderDto
               {
                   OrderId = o.InboundRequestId,
                   OrderCode = o.InboundRequestCode,
                   Status = o.Status.ToString(),
                   CreatedAt = o.CreatedAt.ToDateTimeUtc()
               })
               .ToListAsync();

            //danh sach inbound dang waiting for director approval
            dashboard.DirectorInboundOrders = await _unitOfWork.InboundRequestRepository
                .GetAll()
                .Where(o =>
                    o.CreatedAt >= startInstant &&
                    o.CreatedAt <= endInstant &&
                    o.Status == InboundRequestStatus.WaitingForDirectorApproval)
                .Select(o => new DashboardReportDto.OrderDto
                {
                    OrderId = o.InboundRequestId,
                    OrderCode = o.InboundRequestCode,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt.ToDateTimeUtc()
                })
                .ToListAsync();
            switch (role)
            {
                case "Admin":
                case "Director":
                    // Admin & Director see all
                    break;
                case "Accountant":
                    dashboard.OrderSummary = null;
                    break;

                case "Inventory Manager":
                    dashboard.TotalInboundValue = null;
                    dashboard.TotalOutboundValue = null;
                    break;
                case "Sale Admin":
                    break;
            }

            return dashboard;
        }
    }
}
