using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using NodaTime;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using DrugWarehouseManagement.Common;
using NodaTime.Text;
using Mapster;
using FirebaseAdmin.Messaging;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DrugWarehouseManagement.Service.Services
{
    public class InboundService : IInboundService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateInbound(Guid accountId, CreateInboundRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inboundCode = GenerateInboundCode();

            var inbound = request.Adapt<Inbound>();
            inbound.InboundCode = inboundCode;
            inbound.AccountId = accountId;
            inbound.InboundDate = SystemClock.Instance.GetCurrentInstant();
            inbound.Status = InboundStatus.Pending;
            inbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.InboundRepository.CreateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound record created successfully",
            };
        }

        public async Task<BaseResponse> UpdateInboundStatus(Guid accountId, UpdateInboundStatusRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(request.InboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            if (!Enum.IsDefined(typeof(InboundStatus), request.InboundStatus))
            {
                return new BaseResponse { Code = 404, Message = "Invalid inbound status {Pending, InProgess, Completed, Cancelled}" };
            }

            // Update inbound details
            inbound.Status = request.InboundStatus;
            inbound.AccountId = accountId;
            inbound.InboundDate = SystemClock.Instance.GetCurrentInstant();
            inbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            if (request.InboundStatus == InboundStatus.Completed)
            {
                var inboundReport = await _unitOfWork.InboundReportRepository
                .GetByWhere(ir => ir.InboundId == inbound.InboundId && ir.Status == InboundReportStatus.Pending)
                .OrderByDescending(ir => ir.ReportDate)
                .FirstOrDefaultAsync();

                if (inboundReport != null)
                {
                    inboundReport.Status = InboundReportStatus.Completed;
                    await _unitOfWork.InboundReportRepository.UpdateAsync(inboundReport);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            await _unitOfWork.InboundRepository.UpdateAsync(inbound);

            // If status is Completed, create or update Lot entries
            if (request.InboundStatus == InboundStatus.Completed)
            {
                var inboundDetails = await _unitOfWork.InboundDetailRepository.GetAllByInboundIdAsync(inbound.InboundId);
                if (inboundDetails.Any())
                {
                    foreach (var detail in inboundDetails)
                    {
                        var existingLot = await _unitOfWork.LotRepository
                            .GetByWhere(l =>
                                l.LotNumber == detail.LotNumber &&
                                l.ManufacturingDate == detail.ManufacturingDate &&
                                l.ExpiryDate == detail.ExpiryDate &&
                                l.ProductId == detail.ProductId)
                            .AsQueryable()
                            .FirstOrDefaultAsync();

                        if (existingLot != null)
                        {
                            existingLot.Quantity += detail.Quantity;
                            await _unitOfWork.LotRepository.UpdateAsync(existingLot);
                        }
                        else
                        {
                            var newLot = detail.Adapt<Lot>();
                            newLot.WarehouseId = inbound.WarehouseId;
                            newLot.ProviderId = inbound.ProviderId;

                            await _unitOfWork.LotRepository.CreateAsync(newLot);
                        }

                        // Update OpeningStock based on all Lots with the same ProductId
                        var allLotsForProduct = await _unitOfWork.LotRepository
                            .GetByWhere(l => l.ProductId == detail.ProductId)
                            .ToListAsync();

                        var totalStock = allLotsForProduct.Sum(l => l.Quantity) + detail.Quantity;

                        detail.OpeningStock = totalStock;
                        await _unitOfWork.InboundDetailRepository.UpdateAsync(detail);

                    }
                }
            }
            else if (request.InboundStatus == InboundStatus.Cancelled && inbound.Status == InboundStatus.Completed)
            {
                var inboundDetails = await _unitOfWork.InboundDetailRepository.GetAllByInboundIdAsync(inbound.InboundId);
                if (inboundDetails.Any())
                {
                    foreach (var detail in inboundDetails)
                    {
                        var existingLot = await _unitOfWork.LotRepository
                            .GetByWhere(l =>
                                l.LotNumber == detail.LotNumber &&
                                l.ManufacturingDate == detail.ManufacturingDate &&
                                l.ExpiryDate == detail.ExpiryDate &&
                                l.ProductId == detail.ProductId)
                            .AsQueryable()
                            .FirstOrDefaultAsync();

                        if (existingLot != null)
                        {
                            existingLot.Quantity -= detail.Quantity;

                            if (existingLot.Quantity <= 0)
                            {
                                await _unitOfWork.LotRepository.DeleteAsync(existingLot);
                            }
                            else
                            {
                                await _unitOfWork.LotRepository.UpdateAsync(existingLot);
                            }
                        }
                    }
                }

            }

            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound updated status successfully" };
        }

        public async Task<BaseResponse> UpdateInbound(Guid accountId, UpdateInboundRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            // Validate if the inbound exists
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(request.InboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            if (inbound.Status == InboundStatus.Completed)
            {
                return new BaseResponse { Code = 200, Message = "Inbound is completed and can't be update" };
            }

            inbound.AccountId = accountId;
            inbound.InboundDate = SystemClock.Instance.GetCurrentInstant();
            inbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            request.Adapt(inbound);

            if (request.InboundDetails != null)
            {
                // Step 1: Remove all existing details
                var existingDetails = _unitOfWork.InboundDetailRepository.GetByWhere(x => x.InboundId == inbound.InboundId);
                foreach (var detail in existingDetails)
                {
                    await _unitOfWork.InboundDetailRepository.DeleteAsync(detail);
                }
            }

            await _unitOfWork.InboundRepository.UpdateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound updated successfully" };
        }

        public async Task<ViewInbound> GetInboundById(int inboundId)
        {
            var inbound = await _unitOfWork.InboundRepository
                .GetByWhere(i => i.InboundId == inboundId)
                .Include(i => i.InboundDetails)
                        .ThenInclude(i => i.Product)
                .Include(i => i.Provider)
                .Include(i => i.Account)
                .Include(i => i.Warehouse)
                .AsQueryable()
                .FirstOrDefaultAsync();
            if (inbound == null)
            {
                return new ViewInbound();
            }

            var inboundReport = await _unitOfWork.InboundReportRepository
                .GetByWhere(ir => ir.InboundId == inbound.InboundId)
                .Include(a => a.Assets)
                .OrderByDescending(ir => ir.ReportDate)
                .FirstOrDefaultAsync();

            var result = inbound.Adapt<ViewInbound>();
            result.Report = inboundReport.Adapt<ViewInboundReport>();

            return result;
        }

        public async Task<PaginatedResult<ViewInbound>> GetInboundsPaginatedAsync(InboundtQueryPaging request)
        {
            var query = _unitOfWork.InboundRepository
                        .GetAll()
                        .Include(i => i.InboundDetails)
                        .ThenInclude(i => i.Product)
                        .Include(i => i.Provider)
                        .Include(i => i.Account)
                        .Include(i => i.Warehouse)
                        .AsQueryable();

            if (request.IsReportPendingExist)
            {
                var pendingReportInboundIds = await _unitOfWork.InboundReportRepository
                    .GetByWhere(ir => ir.Status == InboundReportStatus.Pending)
                    .Select(ir => ir.InboundId)
                    .Distinct()
                    .ToListAsync();

                query = query.Where(i => pendingReportInboundIds.Contains(i.InboundId));
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();

                if (int.TryParse(searchTerm, out int inboundId))
                {
                    query = query.Where(i =>
                        i.InboundId == inboundId ||
                        EF.Functions.Like(i.InboundCode.ToLower(), $"%{searchTerm}%"));
                }
                else
                {
                    query = query.Where(i =>
                        EF.Functions.Like(i.InboundId.ToString(), $"%{searchTerm}%"));
                }
            }

            if (Enum.IsDefined(typeof(InboundStatus), request.InboundStatus))
            {
                query = query.Where(i => i.Status == request.InboundStatus);
            }

            var pattern = InstantPattern.ExtendedIso;

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                var parseResult = pattern.Parse(request.DateFrom);
                if (parseResult.Success)
                {
                    Instant dateFromInstant = parseResult.Value;
                    query = query.Where(i => i.InboundDate >= dateFromInstant);
                }
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                var parseResult = pattern.Parse(request.DateTo);
                if (parseResult.Success)
                {
                    Instant dateToInstant = parseResult.Value;
                    query = query.Where(i => i.InboundDate <= dateToInstant);
                }
            }

            query = query.OrderByDescending(i => i.InboundDate);

            // Paginate the result
            var paginatedInbounds = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            // Fetch pending InboundReports for the paginated Inbounds
            var inboundIds = paginatedInbounds.Items.Select(i => i.InboundId).ToList();

            var pendingReports = await _unitOfWork.InboundReportRepository
                    .GetByWhere(ir => inboundIds.Contains(ir.InboundId))
                    .Include(ir => ir.Assets)
                    .ToListAsync();

            // Ensure proper formatting of InboundDate
            var viewInbounds = paginatedInbounds.Items.Adapt<List<ViewInbound>>();

            foreach (var viewInbound in viewInbounds)
            {
                // Map pending InboundReport using Adapt
                viewInbound.Report = pendingReports
                    .Where(ir => ir.InboundId == viewInbound.InboundId)
                    .OrderByDescending(ir => ir.ReportDate)
                    .FirstOrDefault()
                    ?.Adapt<ViewInboundReport>();
            }

            return new PaginatedResult<ViewInbound>
            {
                Items = viewInbounds,
                TotalCount = paginatedInbounds.TotalCount,
                PageSize = paginatedInbounds.PageSize,
                CurrentPage = paginatedInbounds.CurrentPage
            };
        }

        private string GenerateInboundCode()
        {
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            string dateDigits = DateTime.Now.ToString("MMdd");
            return $"IC{uniqueId}{dateDigits}";
        }

        public async Task<byte[]> GenerateInboundPdfAsync(int inboundId)
        {
            // Lấy thông tin inbound
            var inbound = await _unitOfWork.InboundRepository
                .GetByWhere(i => i.InboundId == inboundId)
                .Include(i => i.InboundDetails)
                    .ThenInclude(i => i.Product)
                .Include(i => i.Provider)
                .Include(i => i.Warehouse)
                .AsQueryable()
                .FirstOrDefaultAsync();

            if (inbound == null)
            {
                throw new Exception("Inbound not found");
            }

            Settings.License = LicenseType.Community;

            // Tạo PDF document sử dụng QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Times New Roman"));

                    // Header
                    page.Header()
                        .Column(column =>
                        {
                            // Thông tin công ty
                            column.Item()
                                .AlignLeft()
                                .Text("CÔNG TY TNHH DƯỢC PHẨM TRUNG HÀNH")
                                .FontSize(14)
                                .Bold();

                            column.Item()
                                .AlignLeft()
                                .Text("ĐC: 2/35 Chấn Hưng, Phường 6, Quận Tân Bình, Tp Hồ Chí Minh")
                                .FontSize(10);

                            column.Item()
                                .AlignLeft()
                                .Text("ĐT: 0983 139 320 - 028 62 65 2500")
                                .FontSize(10);

                            // Số phiếu và ngày
                            column.Item()
                                .AlignRight()
                                .Text($"SỐ: {inbound.InboundCode}")
                                .FontSize(10);

                            column.Item()
                                .AlignRight()
                                .Text($"Ngày: {inbound.InboundDate.Value.ToString("dd/MM/yyyy HH:mm", null)}")
                                .FontSize(10);

                            // Tiêu đề
                            column.Item()
                                .AlignCenter()
                                .PaddingTop(10)
                                .Text("PHIẾU GIAO NHẬN")
                                .FontSize(16)
                                .Bold();

                            // Khách hàng và địa chỉ
                            column.Item()
                                .PaddingTop(10)
                                .Text($"Khách hàng: {inbound.Provider?.ProviderName ?? "N/A"}")
                                .FontSize(12);

                            column.Item()
                                .Text($"Địa chỉ: {inbound.Provider?.Address ?? "N/A"}")
                                .FontSize(12);

                            column.Item()
                                .Text($"Điện thoại: {inbound.Provider?.PhoneNumber ?? "N/A"}")
                                .FontSize(12);

                            // Số biên nhận và mã vận đơn
                            column.Item()
                                .PaddingTop(5)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text($"SỐ BN: {inbound.InboundCode}")
                                        .FontSize(12);

                                    row.RelativeItem()
                                        .AlignRight()
                                        .Text($"Mã vận đơn: {inbound.InboundCode}")
                                        .FontSize(12);
                                });

                            // Ghi chú
                            column.Item()
                                .PaddingTop(5)
                                .Text($"Ghi chú: TỔI GẦN NHÀ A TRUNG GỌI SỐ 09136993663 GIAO CHO A HOÀNG")
                                .FontSize(12);
                        });

                    // Content (Bảng chi tiết hàng hóa)
                    page.Content()
                        .PaddingVertical(10)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);  // STT
                                columns.RelativeColumn(3);  // Tên hàng
                                columns.RelativeColumn(2);  // Số lô
                                columns.RelativeColumn(2);  // Hạn dùng
                                columns.RelativeColumn(1);  // ĐVT
                                columns.RelativeColumn(1);  // Số lượng
                                columns.RelativeColumn(2);  // Đơn giá
                                columns.RelativeColumn(2);  // Thành tiền
                            });

                            // Định nghĩa CellStyle một lần duy nhất
                            IContainer CellStyle(IContainer container)
                            {
                                return container
                                    .Border(1)
                                    .BorderColor(Colors.Black)
                                    .Padding(5);
                            }

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("STT").AlignCenter();
                                header.Cell().Element(CellStyle).Text("TÊN HÀNG").AlignCenter();
                                header.Cell().Element(CellStyle).Text("SỐ LÔ").AlignCenter();
                                header.Cell().Element(CellStyle).Text("HẠN DÙNG").AlignCenter();
                                header.Cell().Element(CellStyle).Text("ĐVT").AlignCenter();
                                header.Cell().Element(CellStyle).Text("SL").AlignCenter();
                                header.Cell().Element(CellStyle).Text("ĐƠN GIÁ").AlignCenter();
                                header.Cell().Element(CellStyle).Text("THÀNH TIỀN").AlignCenter();
                            });

                            // Table Content
                            int index = 1;
                            decimal totalAmount = 0;
                            foreach (var detail in inbound.InboundDetails)
                            {
                                decimal unitPrice = detail.UnitPrice; // Giả sử có field UnitPrice trong InboundDetail
                                decimal amount = unitPrice * detail.Quantity;
                                totalAmount += amount;

                                table.Cell().Element(CellStyle).Text($"{index++}").AlignCenter();
                                table.Cell().Element(CellStyle).Text(detail.Product?.ProductName ?? "N/A");
                                table.Cell().Element(CellStyle).Text(detail.LotNumber);
                                table.Cell().Element(CellStyle).Text(detail.ExpiryDate?.ToString("dd/MM/yyyy") ?? "N/A");
                                table.Cell().Element(CellStyle).Text(detail.Product?.SKU ?? "N/A").AlignCenter(); // Đơn vị tính
                                table.Cell().Element(CellStyle).Text(detail.Quantity.ToString()).AlignCenter();
                                table.Cell().Element(CellStyle).Text($"{unitPrice:N0}").AlignRight();
                                table.Cell().Element(CellStyle).Text($"{amount:N0}").AlignRight();
                            }

                            // VAT và Tổng cộng
                            decimal vat = 0; // Giả sử VAT 0% như trong mẫu
                                             // Dòng VAT
                            table.Cell().Element(CellStyle).Text("VAT 0%").AlignLeft();
                            table.Cell().ColumnSpan(6).Element(CellStyle).Text("");
                            table.Cell().Element(CellStyle).Text($"{vat:N0}").AlignRight();

                            // Dòng Cộng
                            table.Cell().Element(CellStyle).Text("CỘNG").AlignLeft();
                            table.Cell().ColumnSpan(6).Element(CellStyle).Text("");
                            table.Cell().Element(CellStyle).Text($"{totalAmount:N0}").AlignRight();

                            // Dòng Chiết khấu
                            table.Cell().Element(CellStyle).Text("CHIẾT KHẤU").AlignLeft();
                            table.Cell().ColumnSpan(6).Element(CellStyle).Text("");
                            table.Cell().Element(CellStyle).Text("0").AlignRight();

                            // Dòng Tổng cộng
                            table.Cell().Element(CellStyle).Text("TỔNG CỘNG").AlignLeft();
                            table.Cell().ColumnSpan(6).Element(CellStyle).Text("");
                            table.Cell().Element(CellStyle).Text($"{totalAmount:N0}").AlignRight();
                        });

                    // Footer (Phần ký tên)
                    page.Footer()
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item()
                                .PaddingTop(20)
                                .Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Khách hàng").Bold().AlignCenter();
                                        col.Item().Text("(Ký, họ tên)").AlignCenter();
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Người giao hàng").Bold().AlignCenter();
                                        col.Item().Text("(Ký, họ tên)").AlignCenter();
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Thủ kho").Bold().AlignCenter();
                                        col.Item().Text("(Ký, họ tên)").AlignCenter();
                                    });

                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text("Kế toán").Bold().AlignCenter();
                                        col.Item().Text("(Ký, họ tên)").AlignCenter();
                                    });
                                });

                            column.Item()
                                .PaddingTop(10)
                                .Text("phiếu bàn hàng gửi thủ kho bổc đơn")
                                .FontSize(10)
                                .Italic();
                        });
                });
            });

            // Generate PDF và trả về byte array
            return document.GeneratePdf();
        }
    }
}
