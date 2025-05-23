﻿using Azure;
using Azure.Core;
using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Calendars;
using NodaTime.Text;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net;
using System.Net.Sockets;

namespace DrugWarehouseManagement.Service.Services
{
    public class OutboundService : IOutboundService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OutboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task UpdateCustomerLoyaltyStatusAsync(int customerId)
        {
            // Đếm số đơn xuất không bị hủy của khách hàng
            var count = await _unitOfWork.OutboundRepository
                .GetAll()
                .CountAsync(o => o.CustomerId == customerId && o.Status != OutboundStatus.Cancelled);

            // Ví dụ: >= 5 đơn thì đánh dấu khách hàng thân thiết
            if (count >= 5)
            {
                var customer = await _unitOfWork.CustomerRepository
                    .GetAll()
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                if (customer != null && !customer.IsLoyal)
                {
                    customer.IsLoyal = true;
                    await _unitOfWork.CustomerRepository.UpdateAsync(customer);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }


        public async Task<OutboundResponse> GetOutboundByIdAsync(int outboundId)
        {
            var outbound = await _unitOfWork.OutboundRepository
                .GetByWhere(o => o.OutboundId == outboundId)
                .Include(c => c.Customer)
                .Include(o => o.OutboundDetails)
                .ThenInclude(od => od.Lot)
                .ThenInclude(l => l.Product)

                .Include(o => o.OutboundDetails)
                .ThenInclude(od => od.Lot)
                .ThenInclude(l => l.Warehouse)
                .FirstOrDefaultAsync(o => o.OutboundId == outboundId);
            if (outbound == null)
            {
                throw new Exception("Không tìm thấy đơn xuất.");
            }
            var outboundResponse = outbound.Adapt<OutboundResponse>();
            return outboundResponse;
        }
        public async Task<BaseResponse> CreateOutbound(Guid accountId, CreateOutboundRequest request)
        {
            var response = new BaseResponse();
            var customer = await _unitOfWork.CustomerRepository
               .GetAll()
               .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId);

            if (customer == null)
            {
                throw new Exception("Khách hàng chưa tồn tại. Vui lòng tạo Customer trước khi xuất hàng.");
            }

            // Sinh outbound code
            var generatedOutboundCode = $"OUTB-{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            // Map request sang Outbound entity
            var outbound = request.Adapt<Outbound>();
            outbound.OutboundCode = generatedOutboundCode;
            outbound.Status = OutboundStatus.Pending;
            outbound.AccountId = accountId;

            // Lấy danh sách Lot dựa trên các LotId được gửi từ request
            var lotIds = request.OutboundDetails.Select(d => d.LotId).ToList();
            var lots = await _unitOfWork.LotRepository
                            .GetByWhere(l => lotIds.Contains(l.LotId))
                            .ToListAsync();
            var distinctWarehouses = lots
                            .Select(l => l.WarehouseId)
                            .Distinct()
                            .ToList();
            if (distinctWarehouses.Count > 1)
                throw new Exception("Các sản phẩm trong đơn phải cùng một kho.");

            if (lots.Count != lotIds.Count)
            {
                throw new Exception("Không tìm thấy lô hàng.");
            }


            // Danh sách chứa các chi tiết đơn xuất
            var today = DateOnly.FromDateTime(DateTime.Now);
            var detailsList = new List<OutboundDetails>();

            // Kiểm tra số lượng trong lô và tạo chi tiết đơn xuất
            foreach (var detailRequest in request.OutboundDetails)
            {
                // Tìm Lot tương ứng với LotId trong request
                var lot = lots.FirstOrDefault(l => l.LotId == detailRequest.LotId);
                if (lot == null)
                {
                    throw new Exception($"Lô hàng yêu cầu: {detailRequest.LotId} không tìm thấy.");
                }
                if (lot.ExpiryDate < today)
                {
                    throw new Exception($"Lô {lot.LotNumber} đã hết hạn dùng ({lot.ExpiryDate:dd/MM/yyyy}).");
                }

                if (lot.WarehouseId == 2)
                {
                    throw new Exception($"Lô {lot.LotNumber} đang ở kho hủy, không được phép xuất.");
                }

                if (lot.WarehouseId == 6)
                {
                    throw new Exception($"Lô {lot.LotNumber} đang ở kho tạm, không được phép xuất.");
                }
                // Kiểm tra số lượng trong lô có đủ không
                if (lot.Quantity < detailRequest.Quantity)
                {
                    throw new Exception($"Số lượng trong lô {lot.LotNumber} không đủ. Hiện có: {lot.Quantity}, yêu cầu: {detailRequest.Quantity}");
                }

                var inboundDetail = await _unitOfWork.InboundDetailRepository
                    .GetByWhere(i => i.LotNumber == lot.LotNumber && i.ProductId == lot.ProductId)
                    .FirstOrDefaultAsync();

                if (inboundDetail == null)
                    throw new Exception($"Không tìm thấy giá vốn cho LotNumber {lot.LotNumber}");


                // Tính giá xuất
                decimal unitPrice;
                if (detailRequest.UsePricingFormula)
                {
                    decimal baseCost = inboundDetail.UnitPrice;
                    decimal profitMargin = detailRequest.ProfitMargin ?? 0.2M;
                    decimal taxPercentage = detailRequest.TaxPercentage ?? 0.1M;
                    unitPrice = baseCost * (1 + profitMargin) * (1 + taxPercentage);
                }
                else
                {
                    if (detailRequest.UnitPrice.HasValue)
                    {
                        unitPrice = detailRequest.UnitPrice.Value;
                    }
                    else
                    {
                        unitPrice = inboundDetail.UnitPrice;
                    }
                }
                var detail = new OutboundDetails
                {
                    LotId = detailRequest.LotId,
                    Quantity = detailRequest.Quantity,
                    UnitPrice = unitPrice,
                    ExpiryDate = lot.ExpiryDate,
                    Discount = detailRequest.Discount ?? 0,
                    TotalPrice = detailRequest.Quantity * unitPrice * (1 - (decimal)(detailRequest.Discount ?? 0) / 100)
                };

                detailsList.Add(detail);
            }

            outbound.OutboundDetails = detailsList;
            outbound.CreatedAt = SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.OutboundRepository.CreateAsync(outbound);
            await _unitOfWork.SaveChangesAsync();
            await UpdateCustomerLoyaltyStatusAsync(request.CustomerId);
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Tạo đơn xuất thành công"
            };
        }

        public async Task<PaginatedResult<OutboundResponse>> SearchOutboundsAsync(SearchOutboundRequest request)
        {
            var query = _unitOfWork.OutboundRepository
                        .GetAll()
                        .Include(o => o.Customer)
                        .Include(o => o.OutboundDetails)
                        .ThenInclude(od => od.Lot)
                        .ThenInclude(l => l.Product)
                        .Include(o => o.OutboundDetails)
                        .ThenInclude(od => od.Lot)
                        .ThenInclude(l => l.Warehouse)
                        .AsQueryable();
            if (request.CustomerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == request.CustomerId.Value);
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<OutboundStatus>(request.Status, true, out var parsedStatus))
                {
                    query = query.Where(o => o.Status == parsedStatus);
                }
                else
                {
                    throw new Exception("Trạng thái không hợp lệ.");
                }
            }
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();

                query = query.Where(o =>
                    EF.Functions.Like(o.OutboundCode.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(o.Customer.CustomerName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(o.Customer.PhoneNumber.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(o.OutboundOrderCode.ToLower(), $"%{searchTerm}%")
                );
            }
            if (request.DateFrom != null)
            {
                var dateFrom = InstantPattern.ExtendedIso.Parse(request.DateFrom);
                if (!dateFrom.Success)
                {
                    throw new Exception("DateFrom is invalid ISO format");
                }
                query = query.Where(o => o.OutboundDate >= dateFrom.Value);
            }
            if (request.DateTo != null)
            {
                var dateTo = InstantPattern.ExtendedIso.Parse(request.DateTo);
                if (!dateTo.Success)
                {
                    throw new Exception("DateTo is invalid ISO format");
                }
                query = query.Where(o => o.OutboundDate <= dateTo.Value);
            }
            query = query.OrderByDescending(o => o.OutboundDate);
            var paginatedOutbounds = await query.ToPaginatedResultAsync(request.Page, request.PageSize);
            var outboundResponses = paginatedOutbounds.Items.Adapt<List<OutboundResponse>>();
            // 3) Chuẩn bị danh sách tất cả detailId và trả về
            var allDetailIds = paginatedOutbounds.Items
                .SelectMany(o => o.OutboundDetails)
                .Select(d => d.OutboundDetailsId)
                .ToList();

            var allReturns = await _unitOfWork.ReturnOutboundDetailsRepository
                .GetAll()
                .Where(r => allDetailIds.Contains(r.OutboundDetailsId))
                .Include(r => r.OutboundDetails)
                    .ThenInclude(od => od.Lot)
                        .ThenInclude(l => l.Product)
                .Include(r => r.OutboundDetails)
                    .ThenInclude(od => od.Outbound)
                .ToListAsync();
            // 4) Gán Returns cho từng detail trong từng outbound response
            foreach (var outboundDto in outboundResponses)
            {
                foreach (var d in outboundDto.OutboundDetails)
                {
                    d.Returns = allReturns
                        .Where(r => r.OutboundDetailsId == d.OutboundDetailsId)
                        .Select(r =>
                        {
                            var dto = r.Adapt<ReturnOutboundDetailsResponse>();
                            dto.OutboundDetailId = r.OutboundDetailsId;
                            dto.OutboundCode = r.OutboundDetails.Outbound.OutboundCode;
                            dto.ProductCode = r.OutboundDetails.Lot.Product.ProductCode;
                            dto.ProductName = r.OutboundDetails.Lot.Product.ProductName;
                            return dto;
                        })
                        .ToList();
                }
            }
            // Create a new PaginatedResult with mapped DTOs.
            return new PaginatedResult<OutboundResponse>
            {
                Items = outboundResponses,
                TotalCount = paginatedOutbounds.TotalCount,
                PageSize = paginatedOutbounds.PageSize,
                CurrentPage = paginatedOutbounds.CurrentPage
            };
        }
        public async Task<BaseResponse> UpdateOutbound(int outboundId, UpdateOutboundRequest request)
        {
            var outbound = await _unitOfWork.OutboundRepository
                .GetByWhere(o => o.OutboundId == outboundId)
                .Include(o => o.OutboundDetails)
                .FirstOrDefaultAsync(o => o.OutboundId == outboundId);
            if (outbound.Status == OutboundStatus.Cancelled)
                throw new Exception("Đơn xuất đã bị hủy, không thể cập nhật.");
            if (outbound == null)
                throw new Exception("Không tìm thấy đơn xuất.");
            if (request.Status.HasValue)
            {
                var newStatus = request.Status.Value;
                if (newStatus != outbound.Status)
                {
                    switch (newStatus)
                    {
                        case OutboundStatus.Cancelled:
                            if (outbound.Status != OutboundStatus.Pending && outbound.Status != OutboundStatus.InProgress)
                                throw new Exception("Chỉ được phép hủy đơn xuất khi đang ở trạng thái Pending hoặc InProgress.");

                            outbound.Status = OutboundStatus.Cancelled;
                            break;

                        case OutboundStatus.InProgress:
                            if (outbound.Status != OutboundStatus.Pending)
                                throw new Exception("Chỉ được phép chuyển từ Pending sang InProgress.");
                            outbound.Status = OutboundStatus.InProgress;
                            break;

                        case OutboundStatus.Completed:
                            if (outbound.Status != OutboundStatus.InProgress)
                                throw new Exception("Chỉ được phép chuyển từ InProgress sang Completed.");
                            foreach (var detail in outbound.OutboundDetails)
                            {
                                var lot = await _unitOfWork.LotRepository.GetByIdAsync(detail.LotId);
                                if (lot.Quantity < detail.Quantity)
                                    throw new Exception($"Lô {lot.LotNumber} không đủ tồn để hoàn thành đơn.");
                                lot.Quantity -= detail.Quantity;
                                await _unitOfWork.LotRepository.UpdateAsync(lot);
                            }
                            outbound.Status = OutboundStatus.Completed;
                            outbound.OutboundDate = SystemClock.Instance.GetCurrentInstant();
                            break;

                        default:
                            throw new Exception("Trạng thái cập nhật không hợp lệ.");
                    }
                }
            }

            request.Adapt(outbound);
            outbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.OutboundRepository.UpdateAsync(outbound);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Cập nhật đơn xuất thành công"
            };
        }

        public async Task<Outbound> GetOutboundByIdWithDetailsAsync(int outboundId)
        {
            var outbound = await _unitOfWork.OutboundRepository
                .GetByWhere(o => o.OutboundId == outboundId)
                .Include(c => c.Customer)
                .Include(o => o.OutboundDetails)
                 .ThenInclude(od => od.Lot)
                .ThenInclude(l => l.Product)
                .FirstOrDefaultAsync(o => o.OutboundId == outboundId);
            return outbound;
        }
        public async Task<byte[]> GenerateOutboundInvoicePdfAsync(int outboundId)
        {
            var outbound = await GetOutboundByIdWithDetailsAsync(outboundId);
            if (outbound == null)
                throw new Exception("Không tìm thấy đơn xuất.");

            Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(11));

                    // Header...
                    page.Header().Column(col =>
                    {
                        col.Spacing(2);
                        col.Item().Text("CÔNG TY TNHH DƯỢC PHẨM TRUNG HẠNH")
                                     .FontSize(14).SemiBold();
                        col.Item().Text("Đ/c: 2/35 Châu Hưng, P.6, Quận Tân Bình, Tp.Hồ Chí Minh");
                        col.Item().Text("ĐT: 0993 129 300");
                    });

                    // Content
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // Title
                        col.Item().AlignCenter()
                                 .Text("PHIẾU GIAO NHẬN / PHIẾU BÁN HÀNG")
                                 .FontSize(16).Bold();

                        // Customer & Invoice info
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Tên khách hàng: {outbound.ReceiverName}");
                                c.Item().Text($"Địa chỉ: {outbound.ReceiverAddress}");
                                c.Item().Text($"SĐT: {outbound.ReceiverPhone}");
                            });
                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().Text($"Mã phiếu: {outbound.OutboundCode}");
                                c.Item().Text($"Ngày: {outbound.OutboundDate?.ToDateTimeUtc():dd/MM/yyyy}");
                            });
                        });

                        // Details table with Discount
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(def =>
                            {
                                def.ConstantColumn(40);
                                def.RelativeColumn(2);
                                def.RelativeColumn(2);
                                def.RelativeColumn(2);
                                def.RelativeColumn(2);
                                def.RelativeColumn(2);
                                def.RelativeColumn(2);
                                def.RelativeColumn(2);
                                def.RelativeColumn(3);
                            });

                            // Header
                            table.Header(header =>
                            {
                                string[] headers = {
                            "STT", "Tên hàng", "Số lô", "Hạn dùng",
                            "ĐVT", "Số lượng", "Đơn giá", "Chiết khấu (%)", "Thành tiền"
                        };
                                foreach (var text in headers)
                                    header.Cell()
                                          .Border(1).BorderColor(Colors.Black)
                                          .Padding(5).AlignCenter().DefaultTextStyle(x => x.Bold())
                                          .Text(text);
                            });

                            // Rows
                            int stt = 1;
                            foreach (var d in outbound.OutboundDetails)
                            {
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(stt.ToString());
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.Lot.Product.ProductName);
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.Lot.LotNumber);
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.ExpiryDate.ToString("dd/MM/yyyy"));
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.Lot.Product.SKU);
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.Quantity.ToString());
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.UnitPrice.ToString("N0"));
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text((d.Discount).ToString("N0"));
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(5).AlignCenter().Text(d.TotalPrice.ToString("N0"));
                                stt++;
                            }

                            // Footer
                            table.Footer(footer =>
                            {
                                footer.Cell()
                                      .ColumnSpan(8)
                                      .Border(1).BorderColor(Colors.Black)
                                      .Padding(5)
                                      .AlignCenter()
                                      .DefaultTextStyle(x => x.Bold())
                                      .Text("Tổng cộng:");

                                // Ô tính tổng nằm ở cột cuối
                                footer.Cell()
                                      .Border(1).BorderColor(Colors.Black)
                                      .Padding(5)
                                      .AlignCenter()
                                      .DefaultTextStyle(x => x.Bold())
                                      .Text(outbound.OutboundDetails.Sum(x => x.TotalPrice).ToString("N0"));
                            });
                        });

                        // Space before signatures
                        col.Item().PaddingTop(30);

                        // Signature row
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(sign =>
                            {
                                sign.Item().Text("Thủ kho").Bold();
                                sign.Item().Height(50);
                                sign.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                            });
                            row.RelativeItem().Column(sign =>
                            {
                                sign.Item().Text("Người giao hàng").Bold();
                                sign.Item().Height(50);
                                sign.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                            });
                            row.RelativeItem().Column(sign =>
                            {
                                sign.Item().Text("Khách hàng").Bold();
                                sign.Item().Height(50);
                                sign.Item().Text("(Ký, ghi rõ họ tên)").Italic().FontSize(9);
                            });
                        });
                    });

                    // Footer page numbers
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Trang ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });

            });

            return document.GeneratePdf();
        }
    }
}

