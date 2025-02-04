using DrugWarehouseManagement.Common.Enums;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class OutboundService : IOutboundService
    {
        private readonly IUnitOfWork _unitOfWork;
        public OutboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private async Task<string> GenerateOutboundCodeAsync()
        {
            // Define a prefix for the code, such as "OUTB" (Outbound)
            var prefix = "OUTB";
            var random = new Random();
            string newOutboundCode = string.Empty;
            bool isUnique = false;

            while (!isUnique)
            {
                // Generate a random 4-digit number
                var randomNumber = random.Next(1000, 10000);

                // Combine the prefix and the random number to create the OutboundCode
                newOutboundCode = $"{prefix}-{randomNumber}";

                // Check if the generated code already exists in the database
                var existingOutbound = await _unitOfWork.OutboundRepository
                    .GetAll()
                    .FirstOrDefaultAsync(o => o.OutboundCode == newOutboundCode);

                if (existingOutbound == null)
                {
                    // Code is unique
                    isUnique = true;
                }
            }
            return newOutboundCode;
        }
        public async Task<BaseResponse> CreateOutbound(Guid accountId, CreateOutboundRequest request)
        {
            var response = new BaseResponse();
            try
            {
                // Sinh outbound code
                var generatedOutboundCode = await GenerateOutboundCodeAsync();

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
                    return new BaseResponse
                    {
                        Code = (int)HttpStatusCode.NotFound,
                        Message = "One or more Lot IDs not found"
                    };
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
                        return new BaseResponse
                        {
                            Code = (int)HttpStatusCode.NotFound,
                            Message = $"LotId {detailRequest.LotId} not found."
                        };
                    }
                    if (string.IsNullOrEmpty(lot.LotNumber))
                    {
                        return new BaseResponse
                        {
                            Code = (int)HttpStatusCode.BadRequest,
                            Message = $"LotNumber is missing for LotId {lot.LotId}."
                        };
                    }
                    // Kiểm tra số lượng trong lô có đủ không
                    if (lot.Quantity < detailRequest.Quantity)
                    {
                        return new BaseResponse
                        {
                            Code = (int)HttpStatusCode.BadRequest,
                            Message = $"Số lượng trong lô {lot.LotNumber} không đủ. Hiện có: {lot.Quantity}, yêu cầu: {detailRequest.Quantity}"
                        };
                    }

                    // Nếu hợp lệ, trừ số lượng trong Lot
                    lot.Quantity -= detailRequest.Quantity;
                  await  _unitOfWork.LotRepository.UpdateAsync(lot);

                    // Tạo đối tượng OutboundDetails
                    var detail = new OutboundDetails
                    {
                        LotId = detailRequest.LotId,
                        Quantity = detailRequest.Quantity,
                        UnitPrice = detailRequest.UnitPrice,
                        UnitType = detailRequest.UnitType,
                        LotNumber = lot.LotNumber,       // Lấy từ Lot entity
                        ExpiryDate = lot.ExpiryDate,
                        TotalPrice = detailRequest.Quantity * detailRequest.UnitPrice,
                        Status = OutboundDetailStatus.Pending,
                        ProductId = lot.ProductId        // Lấy ProductId từ Lot                                          
                    };

                    detailsList.Add(detail);
                }
                // Liên kết danh sách chi tiết vào navigation property của Outbound
                outbound.OutboundDetails = detailsList;

                // Thêm Outbound vào context
                await _unitOfWork.OutboundRepository.CreateAsync(outbound);

                // Lưu các thay đổi cho cả Outbound, OutboundDetails và cập nhật Lot
                await _unitOfWork.SaveChangesAsync();

                return new BaseResponse
                {
                    Code = (int)HttpStatusCode.OK,
                    Message = "Outbound created successfully"
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Code = 500,
                    Message = "Failed to create outbound",
                    Details = ex.Message
                };
            }
        }
    }
}
