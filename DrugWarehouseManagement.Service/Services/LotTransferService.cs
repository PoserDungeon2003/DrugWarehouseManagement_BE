﻿using Azure.Core;
using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class LotTransferService : ILotTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMinioService _minioService;
        private const string _bucketName = "drugwarehouse";

        public LotTransferService(
            IUnitOfWork unitOfWork,
            IMinioService minioService
            )
        {
            _unitOfWork = unitOfWork;
            _minioService = minioService;
        }

        public async Task<BaseResponse> CreateLotTransfer(Guid accountId, LotTransferRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản");
            }

            var fromWarehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(request.FromWareHouseId);
            var toWarehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(request.ToWareHouseId);

            if (fromWarehouse == null || toWarehouse == null)
            {
                throw new Exception("Không tìm thấy kho hàng");
            }

            if (fromWarehouse.Status == Common.WarehouseStatus.Inactive || toWarehouse.Status == Common.WarehouseStatus.Inactive)
            {
                throw new Exception("Kho hàng không hoạt động");
            }

            var groupedDetails = request.LotTransferDetails
                .GroupBy(l => new { l.LotId })
                .Select(l => new LotTransferDetailRequest
                {
                    LotId = l.Key.LotId,
                    Quantity = l.Sum(d => d.Quantity),
                }).ToList();

            foreach (var detail in groupedDetails)
            {
                var lot = await _unitOfWork.LotRepository
                            .GetByIdAsync(detail.LotId);

                if (lot == null)
                {
                    throw new Exception("Không tìm thấy lô hàng");
                }

                if (detail.Quantity <= 0)
                {
                    throw new Exception("Số lượng không hợp lệ");
                }

                if (lot.Quantity < detail.Quantity)
                {
                    throw new Exception("Số lượng lô hàng không đủ để chuyển");
                }

                if (request.ToWareHouseId == lot.WarehouseId)
                {
                    throw new Exception("Không thể chuyển lô hàng đến kho hiện tại");
                }

                lot.Quantity -= detail.Quantity;
                await _unitOfWork.LotRepository.UpdateAsync(lot);

                // Kiểm tra có lô hàng nào trong kho tới trùng thông tin không
                var getLotInDestWarehouse = await _unitOfWork.LotRepository
                                    .GetByWhere(l => l.LotNumber == lot.LotNumber && 
                                                l.WarehouseId == request.ToWareHouseId &&
                                                l.ManufacturingDate == lot.ManufacturingDate &&
                                                l.ExpiryDate == lot.ExpiryDate && 
                                                l.ProductId == lot.ProductId)
                                    .FirstOrDefaultAsync();

                // Nếu có thì cộng thêm số lượng
                if (getLotInDestWarehouse != null)
                {
                    getLotInDestWarehouse.Quantity += detail.Quantity;
                    await _unitOfWork.LotRepository.UpdateAsync(getLotInDestWarehouse);
                    continue;
                }

                // Tạo lô mới ứng với toWareHouseId
                var newLot = new Lot
                {
                    ProductId = lot.ProductId,
                    Quantity = detail.Quantity,
                    ExpiryDate = lot.ExpiryDate,
                    WarehouseId = request.ToWareHouseId,
                    LotNumber = lot.LotNumber,
                    ManufacturingDate = lot.ManufacturingDate,
                    ProviderId = lot.ProviderId,
                };

                await _unitOfWork.LotRepository.CreateAsync(newLot);
            }

            var lotTransfer = request.Adapt<LotTransfer>();
            lotTransfer.LotTransferDetails = groupedDetails.Adapt<List<LotTransferDetail>>();
            lotTransfer.AccountId = accountId;
            lotTransfer.LotTransferCode = $"LT-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            lotTransfer.LotTransferStatus = LotTransferStatus.Completed;

            await _unitOfWork.LotTransferRepository.CreateAsync(lotTransfer);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Tạo phiếu chuyển kho thành công",
                Result = lotTransfer.Adapt<CreateLotTransferResponse>(),
            };
        }

        public async Task<byte[]> ExportLotTransfer(Guid accountId, int lotTransferId)
        {
            var lotTransfer = await _unitOfWork.LotTransferRepository
                                        .GetByWhere(lt => lt.LotTransferId == lotTransferId)
                                        .Include(w => w.FromWareHouse)
                                        .Include(w => w.ToWareHouse)
                                        .Include(lt => lt.LotTransferDetails)
                                            .ThenInclude(l => l.Lot)
                                                .ThenInclude(p => p.Product)
                                        .Include(lt => lt.LotTransferDetails)
                                            .ThenInclude(l => l.Lot)
                                                .ThenInclude(p => p.Provider)
                                        .FirstOrDefaultAsync();

            if (lotTransfer == null)
            {
                throw new Exception("Không tìm thấy phiếu chuyển kho");
            }

            var totalQuantity = lotTransfer.LotTransferDetails.Sum(l => l.Quantity);

            Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(8);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("CTY TNHH DUOC PHAM TRUNG HANH")
                        .SemiBold().FontSize(14).AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text("PHIẾU CHUYỂN KHO").Bold().FontSize(16).AlignCenter();
                        col.Item().Text($"Mã CT: {lotTransfer.LotTransferCode}      Ngày CT: {lotTransfer.CreatedAt.ToString("dd/MM/yyyy", null)}");
                        col.Item().Text($"Từ kho: {lotTransfer.FromWareHouse.WarehouseName}      Đến kho: {lotTransfer.ToWareHouse.WarehouseName}");

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).Text("STT").Bold();
                                header.Cell().Border(1).Text("Tên mặt hàng").Bold();
                                header.Cell().Border(1).Text("Mã").Bold();
                                header.Cell().Border(1).Text("Lô").Bold();
                                header.Cell().Border(1).Text("HSD").Bold();
                                header.Cell().Border(1).Text("Dãy").Bold();
                                header.Cell().Border(1).Text("ĐVT").Bold();
                                header.Cell().Border(1).Text("SL").Bold();
                                header.Cell().Border(1).Text("Giá").Bold();
                            });

                            int index = 0;

                            foreach (var detail in lotTransfer.LotTransferDetails)
                            {
                                table.Cell().Border(1).Text($"{index + 1}");
                                table.Cell().Border(1).Text($"{detail.Lot.Product.ProductName}");
                                table.Cell().Border(1).Text($"{detail.Lot.Product.ProductCode}");
                                table.Cell().Border(1).Text($"{detail.Lot.LotNumber}");
                                table.Cell().Border(1).Text($"{detail.Lot.ExpiryDate.ToString("dd/MM/yyyy")}");
                                table.Cell().Border(1).Text($"{detail.Lot.Provider.ProviderName}");
                                table.Cell().Border(1).Text($"{detail.Lot.Product.SKU.ToUpper()}");
                                table.Cell().Border(1).Text($"{detail.Quantity.ToString("N0", new CultureInfo("vi-VN"))}");
                                table.Cell().Border(1).Text("0");
                                index++;
                            }

                        });

                        col.Item().Text($"Tổng cộng: {totalQuantity}").Bold();
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Người lập\n\n\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Thủ kho\n\n\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("KTT\n\n\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Ca trưởng\n\n\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Giám sát\n\n\n\n(Ký, họ tên)").AlignCenter();
                        row.RelativeItem().Text("Giám đốc\n\n\n\n(Ký, họ tên)").AlignCenter();
                    });
                });
            }).GeneratePdf();

            // string fileName = $"lot-transfer/{lotTransfer.LotTransferCode}.pdf";
            // var asset = new Asset
            // {
            //     FileUrl = Utils.BuildDownloadAssetUrl("lot-transfer", lotTransfer.LotTransferId.ToString(), fileName),
            //     FileName = $"{lotTransfer.LotTransferCode}.pdf",
            //     FileExtension = "pdf",
            //     FileSize = pdfBytes.Length,
            //     AccountId = accountId,
            //     LotTransfer = lotTransfer,
            // };

            // await _unitOfWork.AssetRepository.CreateAsync(asset);
            // await _unitOfWork.SaveChangesAsync();

            // using (var stream = new MemoryStream(pdfBytes))
            // {
            //     var formFile = new FormFile(
            //         baseStream: stream,
            //         baseStreamOffset: 0,
            //         length: stream.Length,
            //         name: "file",
            //         fileName: $"{lotTransfer.LotTransferCode}.pdf"
            //     );

            //     // Upload to MinIO
            //     await _minioService.FileUpload(_bucketName, formFile, fileName, "application/pdf");
            // }
            return pdfBytes;
        }

        public async Task<ViewLotTransfer> GetLotTransferById(int lotTransferId)
        {
            var lotTransfer = await _unitOfWork.LotTransferRepository
                                    .GetByWhere(lt => lt.LotTransferId == lotTransferId)
                                    .Include(a => a.Account)
                                    .Include(w => w.FromWareHouse)
                                    .Include(w => w.ToWareHouse)
                                    .Include(lt => lt.LotTransferDetails)
                                        .ThenInclude(ltd => ltd.Lot)
                                            .ThenInclude(p => p.Product)
                                    .FirstOrDefaultAsync();
            if (lotTransfer == null)
            {
                throw new Exception("Không tìm thấy phiếu chuyển kho");
            }

            return lotTransfer.Adapt<ViewLotTransfer>();
        }

        public async Task<PaginatedResult<ViewLotTransfer>> GetLotTransfers(LotTransferQueryPaging queryPaging)
        {
            var lotTransfers = _unitOfWork.LotTransferRepository
                                    .GetAll()
                                    .Include(w => w.FromWareHouse)
                                    .Include(w => w.ToWareHouse)
                                    .Include(a => a.Account)
                                    .Include(ltd => ltd.LotTransferDetails)
                                        .ThenInclude(l => l.Lot)
                                            .ThenInclude(p => p.Provider)
                                            .ThenInclude(p => p.Products)
                                    .OrderByDescending(lt => lt.UpdatedAt.HasValue)
                                    .ThenByDescending(lt => lt.UpdatedAt)
                                    .ThenByDescending(lt => lt.CreatedAt)
                                    .AsSplitQuery()
                                    //.Where(lt => lt.LotTransferStatus != Common.LotTransferStatus.Cancelled)
                                    .AsQueryable();

            if (!string.IsNullOrEmpty(queryPaging.Search))
            {
                lotTransfers = lotTransfers
                                .Where(lt =>
                                    lt.LotTransferCode.ToLower().Contains(queryPaging.Search.ToLower().Trim()) ||
                                    lt.FromWareHouse.WarehouseName.ToLower().Contains(queryPaging.Search.ToLower().Trim()) ||
                                    lt.ToWareHouse.WarehouseName.ToLower().Contains(queryPaging.Search.ToLower().Trim())
                                );
            }

            if (queryPaging.DateFrom != null)
            {
                var dateFrom = InstantPattern.ExtendedIso.Parse(queryPaging.DateFrom);
                if (!dateFrom.Success)
                {
                    throw new Exception("DateFrom is invalid ISO format");
                }
                lotTransfers = lotTransfers.Where(lt => lt.CreatedAt >= dateFrom.Value);
            }

            if (queryPaging.DateTo != null)
            {
                var dateTo = InstantPattern.ExtendedIso.Parse(queryPaging.DateTo);
                if (!dateTo.Success)
                {
                    throw new Exception("DateTo is invalid ISO format");
                }
                lotTransfers = lotTransfers.Where(lt => lt.CreatedAt <= dateTo.Value);
            }

            if (queryPaging.Status != null)
            {
                lotTransfers = lotTransfers.Where(lt => lt.LotTransferStatus == queryPaging.Status);
            }

            var result = await lotTransfers.ToPaginatedResultAsync(queryPaging.Page, queryPaging.PageSize);

            return result.Adapt<PaginatedResult<ViewLotTransfer>>();
        }

        public async Task<BaseResponse> UpdateLotTransfer(Guid accountId, UpdateLotTransferRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Không tìm thấy tài khoản");
            }

            var lotTransfer = await _unitOfWork.LotTransferRepository.GetByWhere(lt => lt.LotTransferId == request.LotTransferId)
                                                                    .Include(ltd => ltd.LotTransferDetails)
                                                                    .FirstOrDefaultAsync();

            if (lotTransfer == null)
            {
                throw new Exception("Không tìm thấy phiếu chuyển kho");
            }

            if (request.LotTransferStatus != null)
            {
                if (request.LotTransferStatus == LotTransferStatus.Cancelled)
                {
                    if (lotTransfer.LotTransferStatus == Common.LotTransferStatus.Cancelled)
                    {
                        throw new Exception("Phiếu chuyển kho đã bị huỷ trước đó");
                    }

                    if (lotTransfer.LotTransferStatus != LotTransferStatus.Pending)
                    {
                        throw new Exception("Không thể huỷ phiếu chuyển kho đã được duyệt");
                    }

                    if (lotTransfer.LotTransferDetails != null && lotTransfer.LotTransferDetails.Any())
                    {
                        foreach (var detail in lotTransfer.LotTransferDetails)
                        {
                            var lot = await _unitOfWork.LotRepository.GetByIdAsync(detail.LotId);
                            lot.Quantity += detail.Quantity; // Trả lại số lượng lô hàng
                            await _unitOfWork.LotRepository.UpdateAsync(lot);
                        }
                    }

                    lotTransfer.LotTransferStatus = Common.LotTransferStatus.Cancelled;
                }

            }
            lotTransfer.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            request.Adapt(lotTransfer);

            await _unitOfWork.LotTransferRepository.UpdateAsync(lotTransfer);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Cập nhật phiếu chuyển kho thành công",
            };
        }
    }
}
