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
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class InboundRequestService : IInboundRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InboundRequestService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateInboundRequest(Guid accountId, CreateInboundOrderRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var inboundRequestCode = GenerateInboundRequestCode();

            var inboundRequest = request.Adapt<InboundRequest>();
            inboundRequest.InboundRequestCode = inboundRequestCode;
            inboundRequest.AccountId = accountId;
            inboundRequest.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.InboundRequestRepository.CreateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound Request record created successfully",
            };
        }

        public async Task<BaseResponse> DeleteInboundRequest(Guid accountId, int inboundRequestId)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(inboundRequestId);
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound Request not found" };
            }

            inboundRequest.Status = InboundRequestStatus.Cancelled;
            // Delete inbound record
            await _unitOfWork.InboundRequestRepository.UpdateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound Request deleted successfully" };
        }

        public async Task<ViewInboundRequest> GetInboundRequestById(int inboundRequestId)
        {
            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(inboundRequestId);
            if (inboundRequest == null)
            {
                throw new Exception("Inbound not found");
            }
            var result = inboundRequest.Adapt<ViewInboundRequest>();

            return result;
        }

        public async Task<PaginatedResult<ViewInboundRequest>> GetInboundRequestsPaginatedAsync(QueryPaging request)
        {
            var query = _unitOfWork.InboundRequestRepository
                        .GetAll()
                        .Include(i => i.InboundRequestDetails)
                        .Where(i => i.Status != InboundRequestStatus.Cancelled)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();

                if (int.TryParse(searchTerm, out int inboundId))
                {
                    query = query.Where(i =>
                        i.InboundRequestId == inboundId ||
                        EF.Functions.Like(i.InboundRequestCode.ToLower(), $"%{searchTerm}%"));
                }
                else
                {
                    query = query.Where(i =>
                        EF.Functions.Like(i.InboundRequestCode.ToLower(), $"%{searchTerm}%"));
                }
            }

            var pattern = InstantPattern.ExtendedIso;

            if (!string.IsNullOrEmpty(request.DateFrom))
            {
                var parseResult = pattern.Parse(request.DateFrom);
                if (parseResult.Success)
                {
                    Instant dateFromInstant = parseResult.Value;
                    query = query.Where(i => i.CreatedAt >= dateFromInstant);
                }
            }

            if (!string.IsNullOrEmpty(request.DateTo))
            {
                var parseResult = pattern.Parse(request.DateTo);
                if (parseResult.Success)
                {
                    Instant dateToInstant = parseResult.Value;
                    query = query.Where(i => i.CreatedAt <= dateToInstant);
                }
            }

            query = query.OrderByDescending(i => i.CreatedAt);

            // Paginate the result
            var paginatedInbounds = await query.ToPaginatedResultAsync(request.Page, request.PageSize);

            var viewInboundRequests = paginatedInbounds.Items.Adapt<List<ViewInboundRequest>>();
            return new PaginatedResult<ViewInboundRequest>
            {
                Items = viewInboundRequests,
                TotalCount = paginatedInbounds.TotalCount,
                PageSize = paginatedInbounds.PageSize,
                CurrentPage = paginatedInbounds.CurrentPage
            };
        }

        public async Task<BaseResponse> UpdateInboundRequest(Guid accountId, UpdateInboundOrderRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            // Validate if the inbound exists
            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(request.InboundOrderId);
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            inboundRequest = request.Adapt<InboundRequest>();
            inboundRequest.AccountId = accountId;
            inboundRequest.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.InboundRequestRepository.UpdateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound Request updated successfully" };
        }

        public async Task<BaseResponse> UpdateInboundRequestStatus(Guid accountId, UpdateInboundOrderStatusRequest request)
        {
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                throw new Exception("Account not found");
            }

            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(request.InboundId);
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            // Update inbound request
            inboundRequest.Status = request.InboundOrderStatus;
            inboundRequest.AccountId = accountId;
            inboundRequest.UpdatedAt = SystemClock.Instance.GetCurrentInstant();

            await _unitOfWork.InboundRequestRepository.UpdateAsync(inboundRequest);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound Request updated status successfully" };
        }

        private string GenerateInboundRequestCode()
        {
            Random random = new Random();
            string randomDigits = random.Next(1000, 9999).ToString(); // Generate 4 random digits
            string dateDigits = DateTime.Now.ToString("MMdd"); // Get 4-digit based on current date (MMDD)

            string inboundRequestCode = $"IRC{randomDigits}{dateDigits}";

            if (!Regex.IsMatch(inboundRequestCode, "^IRC\\d{4}\\d{4}$"))
            {
                throw new InvalidOperationException("Generated InboundCode does not match the required pattern.");
            }

            return inboundRequestCode;
        }
    }
}
