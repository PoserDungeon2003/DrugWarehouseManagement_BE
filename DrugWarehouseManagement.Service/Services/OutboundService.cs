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
                .FirstOrDefaultAsync(o => o.OutboundId == outboundId);
            if (outbound == null)
            {
                throw new Exception("Outbound not found.");
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
            var generatedOutboundCode = $"OUTB-{DateTimeOffset.Now.ToUnixTimeMilliseconds()}";

            // Map request sang Outbound entity
            var outbound = request.Adapt<Outbound>();
            outbound.OutboundCode = generatedOutboundCode;
            outbound.OutboundDate = SystemClock.Instance.GetCurrentInstant();
            outbound.Status = OutboundStatus.Pending;
            outbound.AccountId = accountId;
         
            // Lấy danh sách Lot dựa trên các LotId được gửi từ request
            var lotIds = request.OutboundDetails.Select(d => d.LotId).ToList();
            var lots = await _unitOfWork.LotRepository
                            .GetByWhere(l => lotIds.Contains(l.LotId))
                            .ToListAsync();


            if (lots.Count != lotIds.Count)
            {
                throw new Exception("One or some LotId not found.");
            }

            // Danh sách chứa các chi tiết đơn xuất
            var detailsList = new List<OutboundDetails>();

            // Kiểm tra số lượng trong lô và tạo chi tiết đơn xuất
            foreach (var detailRequest in request.OutboundDetails)
            {
                // Tìm Lot tương ứng với LotId trong request
                var lot = lots.FirstOrDefault(l => l.LotId == detailRequest.LotId);
                if (lot == null)
                {
                    throw new Exception($"LotId {detailRequest.LotId} not found.");
                }
                if (string.IsNullOrEmpty(lot.LotNumber))
                {
                    throw new Exception($"LotNumber is missing for LotId {lot.LotId}.");
                }
                // Kiểm tra số lượng trong lô có đủ không
                if (lot.Quantity < detailRequest.Quantity)
                {
                    throw new Exception($"Số lượng trong lô {lot.LotNumber} không đủ. Hiện có: {lot.Quantity}, yêu cầu: {detailRequest.Quantity}");
                }

                // Nếu hợp lệ, trừ số lượng trong Lot
                lot.Quantity -= detailRequest.Quantity;
                await _unitOfWork.LotRepository.UpdateAsync(lot);
                var detail = new OutboundDetails
                {
                    LotId = detailRequest.LotId,
                    Quantity = detailRequest.Quantity,
                    UnitPrice = detailRequest.UnitPrice,
                    ExpiryDate = lot.ExpiryDate,
                    Discount = detailRequest.Discount ?? 0,
                    TotalPrice = (decimal)detailRequest.Quantity * detailRequest.UnitPrice * (1 - ((decimal)(detailRequest.Discount ?? 0) / 100))

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
                Message = "Outbound created successfully"
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
                    throw new Exception("Status is invalid.");
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

            if (outbound == null)
            {
                throw new Exception("Outbound not found.");
            }

            // Nếu có giá trị cập nhật trạng thái trong request
            if (request.Status.HasValue)
            {
                var newStatus = request.Status.Value;

                if (newStatus == OutboundStatus.Cancelled)
                {
                    // Cho phép hủy từ Pending hoặc InProgress
                    if (outbound.Status != OutboundStatus.Pending && outbound.Status != OutboundStatus.InProgress)
                    {
                        throw new Exception("Chỉ được phép hủy đơn xuất khi đang ở trạng thái Pending hoặc InProgress.");
                    }

                    // Cộng lại số lượng sản phẩm vào kho khi hủy đơn
                    foreach (var detail in outbound.OutboundDetails)
                    {
                        var lot = await _unitOfWork.LotRepository
                            .GetByWhere(l => l.LotId == detail.LotId)
                            .FirstOrDefaultAsync();

                        if (lot != null)
                        {
                            lot.Quantity += detail.Quantity;
                            await _unitOfWork.LotRepository.UpdateAsync(lot);
                        }
                    }
                    outbound.Status = OutboundStatus.Cancelled;
                }
                else if (newStatus == OutboundStatus.InProgress)
                {
                    if (outbound.Status != OutboundStatus.Pending)
                    {
                        throw new Exception("Chỉ được phép chuyển từ Pending sang InProgress.");
                    }
                    outbound.Status = OutboundStatus.InProgress;
                }
                else if (newStatus == OutboundStatus.Completed)
                {
                    if (outbound.Status != OutboundStatus.InProgress)
                    {
                        throw new Exception("Chỉ được phép chuyển từ InProgress sang Completed.");
                    }
                    outbound.Status = OutboundStatus.Completed;
                }
                else
                {
                    throw new Exception("Trạng thái cập nhật không hợp lệ.");
                }
            }
            outbound.ReceiverAddress = request.Address;
            outbound.ReceiverPhone = request.PhoneNumber;
            outbound.RecivierName = request.CustomerName;
            outbound.OutboundOrderCode = request.OutboundOrderCode;
            outbound.Note = request.Note;
            outbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.OutboundRepository.UpdateAsync(outbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Outbound updated successfully"
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
            // 1) Load the outbound with details
            var outbound = await GetOutboundByIdWithDetailsAsync(outboundId);
            if (outbound == null)
                throw new Exception("Outbound not found.");
            Settings.License = LicenseType.Community;
            // 2) Build the PDF document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily("Times New Roman").FontSize(11));

                    // Header
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
                                c.Item().Text($"Tên khách hàng: {outbound.RecivierName}");
                                c.Item().Text($"Địa chỉ: {outbound.ReceiverAddress}");
                                c.Item().Text($"SĐT: {outbound.ReceiverPhone}");
                            });
                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().Text($"Mã phiếu: {outbound.OutboundCode}");
                                c.Item().Text($"Ngày: {outbound.OutboundDate?.ToDateTimeUtc():dd/MM/yyyy}");
                            });
                        });

                        // Details table
                        col.Item().Table(table =>
                        {
                            // Define columns
                            table.ColumnsDefinition(def =>
                            {
                                def.ConstantColumn(30);   // STT
                                def.RelativeColumn();     // Tên hàng
                                def.RelativeColumn();     // Số lô
                                def.RelativeColumn();     // Hạn dùng
                                def.RelativeColumn();     // ĐVT
                                def.RelativeColumn();     // Số lượng
                                def.RelativeColumn();     // Đơn giá
                                def.RelativeColumn();     // Thành tiền
                            });

                            // Header row
                            table.Header(header =>
                            {
                                string[] headers = { "STT", "Tên hàng", "Số lô", "Hạn dùng", "ĐVT", "Số lượng", "Đơn giá", "Thành tiền" };
                                foreach (var text in headers)
                                    header.Cell().Element(CellStyle).Text(text);

                                static IContainer CellStyle(IContainer c) =>
                                    c.Border(1).BorderColor(Colors.Grey.Lighten2)
                                     .Padding(5).AlignCenter().DefaultTextStyle(x => x.Bold());
                            });

                            // Data rows
                            int stt = 1;
                            foreach (var d in outbound.OutboundDetails)
                            {
                                table.Cell().Element(CellStyle).Text(stt.ToString());
                                table.Cell().Element(CellStyle).Text(d.Lot.Product.ProductName ?? "N/A");
                                table.Cell().Element(CellStyle).Text(d.Lot.LotNumber);
                                table.Cell().Element(CellStyle).Text(d.ExpiryDate.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(d.Lot.Product.SKU);
                                table.Cell().Element(CellStyle).Text(d.Quantity.ToString());
                                table.Cell().Element(CellStyle).Text(d.UnitPrice.ToString("N2"));
                                table.Cell().Element(CellStyle).Text(d.TotalPrice.ToString("N2"));
                                stt++;

                                static IContainer CellStyle(IContainer c) =>
                                    c.Border(1).BorderColor(Colors.Grey.Lighten2)
                                     .Padding(5).AlignCenter();
                            }

                            // Footer (total)
                            table.Footer(footer =>
                            {
                                footer.Cell().ColumnSpan(6)
                                      .AlignRight()
                                      .Text("Tổng cộng:")
                                      .Bold();
                                footer.Cell().Element(CellStyle).Text("");
                                footer.Cell().Element(CellStyle)
                                      .Text(outbound.OutboundDetails.Sum(x => x.TotalPrice).ToString("N2"));

                                static IContainer CellStyle(IContainer c) =>
                                    c.Border(1).BorderColor(Colors.Grey.Lighten2)
                                     .Padding(5).AlignCenter().DefaultTextStyle(x => x.Bold());
                            });
                        });
                    });

                    // Footer with page numbers
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("Trang ");
                        txt.CurrentPageNumber();
                        txt.Span(" / ");
                        txt.TotalPages();
                    });
                });
            });

            // 3) Render to PDF bytes
            return document.GeneratePdf();
        }
    }
}

