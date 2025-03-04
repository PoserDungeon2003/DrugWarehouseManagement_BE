using Azure.Core;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class LotTransferService : ILotTransferService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LotTransferService(
            IUnitOfWork unitOfWork
            )
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> ApproveLotTransfer(Guid accountId, string lotTransferCode)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var lotTransfer = await _unitOfWork.LotTransferRepository
                                .GetByWhere(lt => lt.LotTransferCode.Equals(lotTransferCode))
                                .FirstOrDefaultAsync();

            if (lotTransfer == null)
            {
                throw new Exception("Lot transfer not found");
            }

            lotTransfer.LotTransferStatus = Common.LotTransferStatus.InProgress;

            foreach (var detail in lotTransfer.LotTransferDetails)
            {

            }
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> CreateLotTransfer(Guid accountId, LotTransferRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var fromWarehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(request.FromWareHouseId);
            var toWarehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(request.ToWareHouseId);

            if (fromWarehouse == null || toWarehouse == null)
            {
                throw new Exception("Warehouse not found");
            }

            if (fromWarehouse.Status == Common.WarehouseStatus.Inactive || toWarehouse.Status == Common.WarehouseStatus.Inactive)
            {
                throw new Exception("Warehouse is inactive");
            }

            var groupedDetails = request.LotTransferDetails
                .GroupBy(l => new { l.LotNumber, l.ExpiryDate, l.ProductId })
                .Select(l => new LotTransferDetailRequest
                {
                    LotNumber = l.Key.LotNumber,
                    ExpiryDate = l.Key.ExpiryDate,
                    ProductId = l.Key.ProductId,
                    Quantity = l.Sum(d => d.Quantity),
                    UnitType = l.First().UnitType,
                }).ToList();

            foreach (var detail in groupedDetails)
            {
                var lot = await _unitOfWork.LotRepository
                            .GetByWhere(l => l.LotNumber.Equals(detail.LotNumber) && l.ExpiryDate == detail.ExpiryDate && l.WarehouseId == request.FromWareHouseId)
                            .FirstOrDefaultAsync();

                if (lot == null)
                {
                    throw new Exception("Lot not found");
                }
                detail.LotId = lot.LotId;

                var product = await _unitOfWork.ProductRepository.GetByIdAsync(detail.ProductId);

                if (product == null)
                {
                    throw new Exception("Product not found");
                }

                if (detail.ProductId != lot.ProductId)
                {
                    throw new Exception("Product not match with lot");
                }

                if (detail.Quantity <= 0)
                {
                    throw new Exception("Quantity must be greater than 0");
                }

                if (lot.Quantity < detail.Quantity)
                {
                    throw new Exception("Quantity not enough");
                }

                if (request.ToWareHouseId == lot.WarehouseId)
                {
                    throw new Exception("Transfer to the same warehouse");
                }

                lot.Quantity -= detail.Quantity;
                await _unitOfWork.LotRepository.UpdateAsync(lot);

                // Kiểm tra có lô hàng nào trong kho tới trùng thông tin không
                var lotInWarehouseTo = await _unitOfWork.LotRepository
                                    .GetByWhere(l => l.LotNumber == detail.LotNumber && l.ExpiryDate == detail.ExpiryDate && l.WarehouseId == request.ToWareHouseId)
                                    .FirstOrDefaultAsync();

                // Kiểm tra mã lô mới có expiry date đã tồn tại chưa
                //if (searchLotByLotNumber.LotNumber == lot.LotNumber)
                //{
                //    throw new Exception("New lot number must be different from old lot number");
                //}

                // Nếu có thì cộng thêm số lượng
                if (lotInWarehouseTo != null)
                {
                    lotInWarehouseTo.Quantity += detail.Quantity;
                    await _unitOfWork.LotRepository.UpdateAsync(lotInWarehouseTo);
                    continue;
                }

                // Tạo lô mới ứng với toWareHouseId
                var newLot = new Lot
                {
                    ProductId = detail.ProductId,
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

            await _unitOfWork.LotTransferRepository.CreateAsync(lotTransfer);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Create transfer order successfully",
                Result = lotTransfer.Adapt<CreateLotTransferResponse>(),  
            };
        }

        public Task<byte[]> ExportLotTransfer(Guid accountId, int lotTransferId)
        {
            Settings.License = LicenseType.Community;

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("CTY TNHH DUOC PHAM TRUNG HANH")
                        .SemiBold().FontSize(14).AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Text("PHIẾU CHUYỂN KHO").Bold().FontSize(16).AlignCenter();
                        col.Item().Text("Mã CT: TR02250014      Ngày CT: 11/02/2025");
                        col.Item().Text("Từ kho: Kho Nguyễn Giản Thanh      Đến kho: Kho Việt Nam");

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

                            table.Cell().Border(1).Text("1");
                            table.Cell().Border(1).Text("Ameprazol 40 (capsules, B/2bls x 7s)");
                            table.Cell().Border(1).Text("AME40");
                            table.Cell().Border(1).Text("240544");
                            table.Cell().Border(1).Text("28/05/26");
                            table.Cell().Border(1).Text("DƯỢC PHẨM OPV");
                            table.Cell().Border(1).Text("HỘP");
                            table.Cell().Border(1).Text("2,640");
                            table.Cell().Border(1).Text("0");
                        });

                        col.Item().Text($"Tổng cộng: 2640").Bold();
                    });

                    page.Footer().Row(row =>
                    {
                        row.ConstantColumn(100).Text("Người lập\n(Ký, họ tên)").AlignCenter();
                        row.ConstantColumn(100).Text("Thủ kho\n(Ký, họ tên)").AlignCenter();
                        row.ConstantColumn(100).Text("KTT\n(Ký, họ tên)").AlignCenter();
                        row.ConstantColumn(100).Text("Ca trưởng\n(Ký, họ tên)").AlignCenter();
                        row.ConstantColumn(100).Text("Giám sát\n(Ký, họ tên)").AlignCenter();
                        row.RelativeColumn().Text("Giám đốc\n(Ký, họ tên)").AlignCenter();
                    });
                });
            }).GeneratePdf();

            return Task.FromResult(pdfBytes);
        }
    }
}
