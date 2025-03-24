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
                return new BaseResponse { Code = 404, Message = "Invalid inbound status {Pending, InProgess, Completed, Cancelled" };
            }

            // Update inbound details
            inbound.Status = request.InboundStatus;
            inbound.AccountId = accountId;
            inbound.UpdatedAt= SystemClock.Instance.GetCurrentInstant();

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
                            newLot.WarehouseId = inbound.WarehouseId ?? 0;
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
            else if (request.InboundStatus == InboundStatus.Cancelled)
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

            
            inbound.AccountId = accountId;
            inbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            request.Adapt(inbound);

            await _unitOfWork.InboundRepository.UpdateAsync(inbound);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse { Code = 200, Message = "Inbound updated successfully" };
        }

        public async Task<ViewInbound> GetInboundById(int inboundId)
        {
            var inbound = await _unitOfWork.InboundRepository.GetByIdAsync(inboundId);
            if (inbound == null)
            {
                return new ViewInbound();
            }

            var result = inbound.Adapt<ViewInbound>();
            var timeZone = DateTimeZoneProviders.Tzdb["Asia/Ho_Chi_Minh"];
            result.InboundDate = InstantPattern.ExtendedIso.Parse(result.InboundDate)
                .Value.InZone(timeZone).ToString("dd/MM/yyyy HH:mm", null);

            return result;
        }

        public async Task<PaginatedResult<ViewInbound>> GetInboundsPaginatedAsync(QueryPaging request)
        {
            var query = _unitOfWork.InboundRepository
                        .GetAll()
                        .Include(i => i.InboundDetails)
                        .ThenInclude(i => i.Product)
                        .Include(i => i.Provider)
                        .Include(i => i.Account)
                        .Include(i => i.Warehouse)
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
                        EF.Functions.Like(i.InboundId.ToString(), $"%{searchTerm}%"));
                }
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

            // Ensure proper formatting of InboundDate
            var timeZone = DateTimeZoneProviders.Tzdb["Asia/Ho_Chi_Minh"];
            var viewInbounds = paginatedInbounds.Items.Adapt<List<ViewInbound>>();

            foreach (var viewInbound in viewInbounds)
            {
                if (viewInbound.InboundDate != null)
                {
                    viewInbound.InboundDate = InstantPattern.ExtendedIso.Parse(viewInbound.InboundDate)
                        .Value.InZone(timeZone).ToString("dd/MM/yyyy HH:mm", null);
                }
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
            Random random = new Random();
            string randomDigits = random.Next(1000, 9999).ToString(); // Generate 4 random digits
            string dateDigits = DateTime.Now.ToString("MMdd"); // Get 4-digit based on current date (MMDD)

            string inboundCode = $"IC{randomDigits}{dateDigits}";

            if (!Regex.IsMatch(inboundCode, "^IC\\d{4}\\d{4}$"))
            {
                throw new InvalidOperationException("Generated InboundCode does not match the required pattern.");
            }

            return inboundCode;
        }
    }
}
