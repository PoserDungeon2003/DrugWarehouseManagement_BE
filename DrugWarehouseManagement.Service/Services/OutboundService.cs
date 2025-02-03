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
        private readonly IAccountService _accountService;
        public OutboundService(IUnitOfWork unitOfWork, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
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
        public async Task<BaseResponse> CreateOutbound(CreateOutboundRequest request)
        {
            var response = new BaseResponse();
            try
            {
                var generatedOutboundCode = await GenerateOutboundCodeAsync();
                var accountId = await _accountService.GetCurrentAccountIdAsync();

                if (accountId == null)
                {
                    return new BaseResponse
                    {
                        Code = (int)HttpStatusCode.Unauthorized,
                        Message = "User is not logged in"
                    };
                }
                var outbound = request.Adapt<Outbound>();
                outbound.OutboundCode = generatedOutboundCode;
                outbound.OutboundDate = SystemClock.Instance.GetCurrentInstant();
                outbound.Status = OutboundStatus.Pending;

                await _unitOfWork.OutboundRepository.CreateAsync(outbound);
                await _unitOfWork.SaveChangesAsync();

                // Fetch Lot details and map OutboundDetails using Mapster
                var lotIds = request.OutboundDetails.Select(d => d.LotId).ToList();
                var lots = await _unitOfWork.LotRepository.GetByWhere(l => lotIds.Contains(l.LotId)).ToListAsync();
                if (lots.Count != lotIds.Count)
                {
                    return new BaseResponse
                    {
                        Code = (int)HttpStatusCode.NotFound,
                        Message = "One or more Lot IDs not found"
                    };
                }

                var outboundDetailsList = request.OutboundDetails.Adapt<List<OutboundDetails>>();
                foreach (var detail in outboundDetailsList)
                {
                    var lot = lots.First(l => l.LotId == detail.LotId);
                    detail.OutboundId = outbound.OutboundId;
                    detail.LotNumber = lot.LotNumber;
                    detail.ExpiryDate = lot.ExpiryDate;
                    detail.TotalPrice = detail.Quantity * detail.UnitPrice;
                    detail.Status = OutboundDetailStatus.Pending;
                }

                await _unitOfWork.OutboundDetailsRepository.AddRangeAsync(outboundDetailsList);
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
