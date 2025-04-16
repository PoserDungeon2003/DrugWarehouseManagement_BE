using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace DrugWarehouseManagement.Service.Services
{
    public class InventoryCheckService : IInventoryCheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryCheckService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateInventoryCheck(Guid accountId, CreateInventoryCheckRequest request)
        {
            try
            {
                // Validate account
                var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    return new BaseResponse
                    {
                        Code = 404,
                        Message = "Account not found."
                    };
                }

                var inventory = request.Adapt<InventoryCheck>();
                inventory.AccountId = accountId;

                if (request.Details != null && request.Details.Any())
                {
                    foreach (var detail in request.Details)
                    {
                        var lot = await _unitOfWork.LotRepository.GetByIdAsync(detail.LotId);
                        if (lot == null)
                        {
                            return new BaseResponse
                            {
                                Code = 400,
                                Message = $"Lot {detail.LotId} not found."
                            };
                        }

                        // Set CheckQuantity based on input
                        var inventoryDetail = detail.Adapt<InventoryCheckDetail>();
                        switch (detail.Status)
                        {
                            case InventoryCheckStatus.Damaged:
                                inventoryDetail.CheckQuantity = detail.Quantity;
                                inventoryDetail.Reason = detail.Reason ?? "Hàng bị hư hại";

                                lot.Quantity -= detail.Quantity;
                                await _unitOfWork.LotRepository.UpdateAsync(lot);
                                await _unitOfWork.SaveChangesAsync();

                                break;

                            case InventoryCheckStatus.Excess:
                                inventoryDetail.CheckQuantity = detail.Quantity;
                                inventoryDetail.Reason = detail.Reason ?? "Hàng dư";
                                break;

                            case InventoryCheckStatus.Lost:
                                inventoryDetail.CheckQuantity = detail.Quantity;
                                inventoryDetail.Reason = detail.Reason ?? "Hàng mất";

                                lot.Quantity -= detail.Quantity;
                                await _unitOfWork.LotRepository.UpdateAsync(lot);
                                await _unitOfWork.SaveChangesAsync();
                                break;

                            case InventoryCheckStatus.Found:

                                var nearestLot = await _unitOfWork.InventoryCheckDetailRepository
                                    .GetByWhere(ic => ic.LotId == detail.LotId && ic.Status == InventoryCheckStatus.Lost)
                                    .Join(_unitOfWork.InventoryCheckRepository.GetAll(),
                                        icd => icd.InventoryCheckId,
                                        ic => ic.InventoryCheckId,
                                        (icd, ic) => new { Detail = icd, CheckDate = ic.CheckDate })
                                    .OrderByDescending(x => x.CheckDate)
                                    .Select(x => x.Detail)
                                    .FirstOrDefaultAsync();

                                if (nearestLot == null || nearestLot.CheckQuantity < detail.Quantity)
                                {
                                    return new BaseResponse
                                    {
                                        Code = 400,
                                        Message = $"Không thể tìm thấy số lượng {detail.Quantity} cho lô {lot.LotNumber} với số lượng {nearestLot.CheckQuantity}."
                                    };
                                }

                                // Deduct quantity from nearest lot
                                nearestLot.CheckQuantity -= detail.Quantity;
                                await _unitOfWork.InventoryCheckDetailRepository.UpdateAsync(nearestLot);

                                inventoryDetail.CheckQuantity = detail.Quantity;
                                inventoryDetail.Reason = detail.Reason ?? "Hàng mất đã tìm thấy";

                                lot.Quantity += detail.Quantity;
                                await _unitOfWork.LotRepository.UpdateAsync(lot);
                                await _unitOfWork.SaveChangesAsync();

                                break;

                            default:
                                return new BaseResponse
                                {
                                    Code = 400,
                                    Message = $"Invalid status for lot {lot.LotNumber}."
                                };
                        }
                        inventory.InventoryCheckDetails.Add(inventoryDetail);
                    }
                }


                // Save to database
                await _unitOfWork.InventoryCheckRepository.CreateAsync(inventory);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse
                {
                    Code = 200,
                    Message = "Inventory check created successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = 404,
                    Message = $"Failed to create inventory check: {ex.Message}"
                };
            }
        }

        public async Task<byte[]> GenerateInventoryCheckPdfAsync(int inboundId)
        {
            // Fetch the inventory check with related data
            var inventoryCheck = await _unitOfWork.InventoryCheckRepository
                .GetByWhere(ic => ic.InventoryCheckId == inboundId)
                .Include(ic => ic.InventoryCheckDetails)
                    .ThenInclude(icd => icd.Lot)
                    .ThenInclude(icd => icd.Product)
                .Include(ic => ic.Warehouse)
                .Include(ic => ic.Account)
                .FirstOrDefaultAsync();

            if (inventoryCheck == null)
            {
                throw new ArgumentException($"Inventory check with ID {inboundId} not found.");
            }

            // Configure QuestPDF settings (optional, for license)
            QuestPDF.Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Times New Roman"));

                    // Header
                    page.Header()
                        .PaddingBottom(10)
                        .Column(column =>
                        {
                            column.Item()
                                .Text("BÁO CÁO KIỂM KÊ")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2)
                                .AlignCenter();

                            column.Item()
                                .Text($"Mã kiểm kê: {inboundId}")
                                .FontSize(12)
                                .Italic()
                                .AlignCenter();
                        });

                    // Content
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Border(0.5f)
                        .BorderColor(Colors.Grey.Medium)
                        .Padding(10)
                        .Column(column =>
                        {
                            // Metadata section
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Tiêu đề:").Bold();
                                row.RelativeItem().PaddingLeft(5).Text(inventoryCheck.Title);
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Ngày kiểm kê:").Bold();
                                row.RelativeItem().PaddingLeft(5).Text(InstantPattern.CreateWithInvariantCulture("dd/MM/yyyy HH:mm").Format(inventoryCheck.CheckDate));
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Người kiểm kê:").Bold();
                                row.RelativeItem().PaddingLeft(5).Text(inventoryCheck.Account.FullName);
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Kho:").Bold();
                                row.RelativeItem().PaddingLeft(5).Text(inventoryCheck.Warehouse?.WarehouseName ?? "N/A");
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Ghi chú:").Bold();
                                row.RelativeItem().PaddingLeft(5).Text(inventoryCheck.Notes ?? "Không có");
                            });

                            column.Item().PaddingVertical(0.5f, Unit.Centimetre);

                            // Table of details
                            column.Item()
                                .PaddingBottom(5)
                                .Text("Chi tiết kiểm kê")
                                .FontSize(14)
                                .Bold();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(); // Lot Number
                                    columns.RelativeColumn(); // Product Name
                                    columns.RelativeColumn(); // Status
                                    columns.RelativeColumn(); // Quantity
                                    columns.RelativeColumn(); // Reason
                                    columns.RelativeColumn(); // Note
                                });

                                // Table header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Blue.Lighten4).Border(0.5f).Padding(5).Text("Số lô").Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Lighten4).Border(0.5f).Padding(5).Text("Tên sản phẩm").Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Lighten4).Border(0.5f).Padding(5).Text("Trạng thái").Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Lighten4).Border(0.5f).Padding(5).Text("Số lượng").Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Lighten4).Border(0.5f).Padding(5).Text("Lý do").Bold().AlignCenter();
                                    header.Cell().Background(Colors.Blue.Lighten4).Border(0.5f).Padding(5).Text("Ghi chú").Bold().AlignCenter();
                                });

                                // Table rows
                                foreach (var detail in inventoryCheck.InventoryCheckDetails)
                                {
                                    table.Cell().Border(0.5f).Padding(5).Text(detail.Lot?.LotNumber ?? "N/A").AlignCenter();
                                    table.Cell().Border(0.5f).Padding(5).Text(detail.Lot?.Product?.ProductName ?? "N/A").AlignCenter();
                                    table.Cell().Border(0.5f).Padding(5).Text(detail.Status.ToString()).AlignCenter();
                                    table.Cell().Border(0.5f).Padding(5).Text(detail.Quantity.ToString()).AlignCenter();
                                    table.Cell().Border(0.5f).Padding(5).Text(detail.Reason).AlignCenter();
                                    table.Cell().Border(0.5f).Padding(5).Text(detail.Notes ?? "Không có").AlignCenter();
                                }
                            });
                        });

                    // Footer
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                    {
                        x.Span("Trang ").FontSize(10).FontColor(Colors.Grey.Darken1);
                        x.CurrentPageNumber().FontSize(10).FontColor(Colors.Grey.Darken1);
                        x.Span(" / ").FontSize(10).FontColor(Colors.Grey.Darken1);
                        x.TotalPages().FontSize(10).FontColor(Colors.Grey.Darken1);
                        x.Span(" - Ngày tạo: ").FontSize(10).FontColor(Colors.Grey.Darken1);
                        x.Span(InstantPattern.CreateWithInvariantCulture("dd/MM/yyyy").Format(SystemClock.Instance.GetCurrentInstant())).FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return pdfBytes;
        }

        public async Task<PaginatedResult<ViewInventoryCheck>> GetInventoryChecksPaginatedAsync(QueryPaging request)
        {
            var query = _unitOfWork.InventoryCheckRepository
                        .GetAll()
                        .Include(i => i.Warehouse)
                        .Include(i => i.InventoryCheckDetails)
                        .ThenInclude(i => i.Lot)
                        .ThenInclude(i => i.Product)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();

                if (int.TryParse(searchTerm, out int inventoryCheckId))
                {
                    query = query.Where(i =>
                        i.InventoryCheckId == inventoryCheckId ||
                        EF.Functions.Like(i.Title.ToLower(), $"%{searchTerm}%") ||
                        i.InventoryCheckDetails.Any(d => EF.Functions.Like(d.Lot.LotNumber.ToLower(), $"%{searchTerm}%") ||
                        i.InventoryCheckDetails.Any(d => EF.Functions.Like(d.Lot.LotId.ToString(), $"%{searchTerm}%"))));
                }
                else
                {
                    query = query.Where(i =>
                        EF.Functions.Like(i.Title.ToLower(), $"%{searchTerm}%") ||
                        i.InventoryCheckDetails.Any(d => EF.Functions.Like(d.Lot.LotNumber.ToLower(), $"%{searchTerm}%") ||
                        i.InventoryCheckDetails.Any(d => EF.Functions.Like(d.Lot.LotId.ToString(), $"%{searchTerm}%"))));
                }
            }

            var pattern = InstantPattern.ExtendedIso;

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                var parseResult = pattern.Parse(request.DateFrom);
                if (parseResult.Success)
                {
                    Instant dateFromInstant = parseResult.Value;
                    query = query.Where(i => i.CheckDate >= dateFromInstant);
                }
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                var parseResult = pattern.Parse(request.DateTo);
                if (parseResult.Success)
                {
                    Instant dateToInstant = parseResult.Value;
                    query = query.Where(i => i.CheckDate <= dateToInstant);
                }
            }

            query = query.OrderByDescending(i => i.CheckDate);

            // Paginate the result
            var paginatedInbounds = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var viewInventoryChecks = paginatedInbounds.Items.Adapt<List<ViewInventoryCheck>>();

            return new PaginatedResult<ViewInventoryCheck>
            {
                Items = viewInventoryChecks,
                TotalCount = paginatedInbounds.TotalCount,
                PageSize = paginatedInbounds.PageSize,
                CurrentPage = paginatedInbounds.CurrentPage
            };
        }
    }
}
