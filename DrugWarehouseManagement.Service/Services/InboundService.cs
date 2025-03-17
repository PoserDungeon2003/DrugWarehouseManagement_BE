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

namespace DrugWarehouseManagement.Service.Services
{
    public class InboundService : IInboundService
    {
        private readonly IUnitOfWork _unitOfWork;
        public InboundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateInbound(CreateInboundRequest request)
        {
            var inboundCode = GenerateInboundCode();
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Inbound record created successfully",
            };
        }

        public Task<BaseResponse> CreateInboundDetail(CreateInboundRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> UpdateInboundStatus(UpdateInboundStatusRequest request)
        {
            // Validate if the inbound exists
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(request.InboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            // Validate if the account exists
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            // Update inbound details
            inbound.Status = request.InboundStatus;
            inbound.AccountId = request.AccountId;
            inbound.UpdatedAt= SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound updated status successfully" };
        }

        public async Task<BaseResponse> UpdateInbound(UpdateInboundRequest request)
        {
            // Validate if the inbound exists
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(request.InboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            inbound = request.Adapt<Inbound>();

            await _unitOfWork.InboundRepository.UpdateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound updated successfully" };
        }

        public async Task<BaseResponse> DeleteInbound(int inboundId)
        {
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(inboundId);
            if (inbound == null)
            {
                return new BaseResponse { Code = 404, Message = "Inbound not found" };
            }

            // Remove all related inbound details first
            var inboundDetails = await _unitOfWork.InboundDetailRepository.GetAllByInboundIdAsync(inboundId);
            foreach (var detail in inboundDetails)
            {
                detail.Status = InboundDetailStatus.Rejected;
                await _unitOfWork.InboundDetailRepository.UpdateAsync(detail);
            }

            inbound.Status = InboundStatus.Cancelled;
            // Delete inbound record
            await _unitOfWork.InboundRepository.UpdateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound deleted successfully" };
        }

        public async Task<ViewInbound> GetInboundById(int inboundId)
        {
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(inboundId);
            if (inbound == null)
            {
                throw new Exception("Inbound not found");
            }

            var inboundDetails = await _unitOfWork.InboundDetailRepository.GetAllByInboundIdAsync(inboundId);

            return new ViewInbound
            {
                InboundId = inbound.InboundId,
                InboundCode = inbound.InboundCode,
                ProviderOrderCode = inbound.ProviderOrderCode,
                Quantity = inbound.Quantity,
                Price = inbound.Price,
                Note = inbound.Note,
                InboundDate = inbound.InboundDate.ToString(),
                Status = inbound.Status
            };
        }

        public async Task<PaginatedResult<ViewInbound>> GetInboundsPaginatedAsync(QueryPaging request)
        {
            var query = _unitOfWork.InboundRepository
                        .GetAll()
                        .Include(i => i.InboundDetails)  // Assuming Products are related to InboundDetails
                        .AsQueryable();

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
                        EF.Functions.Like(i.InboundCode.ToLower(), $"%{searchTerm}%"));
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

            var viewInbounds = paginatedInbounds.Items.Select(inbound => new ViewInbound
            {
                InboundId = inbound.InboundId,
                InboundCode = inbound.InboundCode,
                ProviderOrderCode = inbound.ProviderOrderCode,
                Quantity = inbound.Quantity,
                Price = inbound.Price,
                Note = inbound.Note,
                InboundDate = inbound.InboundDate.ToString(),
                Status = inbound.Status
            }).ToList();

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
            // Generate 4 random digits
            Random random = new Random();
            string randomDigits = random.Next(1000, 9999).ToString();

            // Get 3 digits from the current time (milliseconds)
            string timeDigits = DateTime.Now.Millisecond.ToString("D3").Substring(0, 3);

            // Combine into the final code
            string inboundCode = $"IC{randomDigits}{timeDigits}";

            // Validate the code with a regex pattern
            string pattern = "^IC\\d{4}\\d{3}$";
            if (!Regex.IsMatch(inboundCode, pattern))
            {
                throw new InvalidOperationException("Generated InboundCode does not match the required pattern.");
            }

            return inboundCode;
        }

    }
}
