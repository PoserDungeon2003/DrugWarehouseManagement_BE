using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using FirebaseAdmin.Messaging;
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
                return new BaseResponse { Code = 404, Message = "Account not found" };
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

        public async Task<ViewInboundRequest> GetInboundRequestById(int inboundRequestId)
        {
            var inboundRequest = await _unitOfWork.InboundRequestRepository
                    .GetByWhere(i => i.InboundRequestId == inboundRequestId)
                    .Include(i => i.InboundRequestDetails)
                    .ThenInclude(i => i.Product)
                    .AsQueryable()
                    .FirstOrDefaultAsync();
            if (inboundRequest == null )
            {
                throw new Exception("Inbound not found");
            }
            var result = inboundRequest.Adapt<ViewInboundRequest>();
            result.CreateDate = InstantPattern.ExtendedIso.Parse(result.CreateDate)
                .Value.ToString("dd/MM/yyyy HH:mm", null);

            return result;
        }

        public async Task<PaginatedResult<ViewInboundRequest>> GetInboundRequestsPaginatedAsync(QueryPaging request)
        {
            var query = _unitOfWork.InboundRequestRepository
                        .GetAll()
                        .Include(i => i.InboundRequestDetails)
                        .ThenInclude(i => i.Product)
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

            foreach (var viewInbound in viewInboundRequests)
            {
                if (viewInbound.CreateDate != null)
                {
                    viewInbound.CreateDate = InstantPattern.ExtendedIso.Parse(viewInbound.CreateDate)
                        .Value.ToString("dd/MM/yyyy HH:mm", null);
                }
            }
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
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            // Validate if the inbound exists
            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(request.InboundOrderId);
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            request.Adapt(inboundRequest);
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
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            var inboundRequest = await _unitOfWork.InboundRequestRepository.GetByIdAsync(request.InboundId);
            if (inboundRequest == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound Request not found" };
            }

            if (!Enum.IsDefined(typeof(InboundRequestStatus), request.InboundOrderStatus))
            {
                return new BaseResponse { Code = 404, Message = "Invalid inbound request status {WaitingForAccountantApproval, WaitingForDirectorApproval, WaitingForSaleAdminApproval, InProgress, WaitingForImport, Completed, Cancelled}" };
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
            var dateDigits = DateTime.Now.ToString("MMdd");
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();
            return $"IRC{uniqueId}{dateDigits}";
        }
    }
}
