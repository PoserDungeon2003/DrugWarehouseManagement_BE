using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using Microsoft.EntityFrameworkCore;
using Azure.Core;
using System.Text.RegularExpressions;
using DrugWarehouseManagement.Common;
using Microsoft.Identity.Client;

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
            //Valid Foreign key in Model property
            var validationResponse = await CheckForeignKey(request);
            if (validationResponse.Code != 200)
            {
                return validationResponse;
            }

            var inboundCode = GenerateInboundCode();

            // Create Inbound entity 
            var inbound = new Inbound
            {
                InboundCode = inboundCode,
                ProviderOrderCode = request.ProviderOrderCode,
                Quantity = request.Quantity,
                Price = request.Price,
                Note = request.Note,
                InboundDate = request.InboundDate,
                ProviderId = request.ProviderId,
                AccountId = request.AccountId,
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                UpdatedAt = SystemClock.Instance.GetCurrentInstant()
            };

            await _unitOfWork.InboundRepository.CreateAsync(inbound);
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

            //Valid Foreign key in Model property
            var validationResponse = await CheckForeignKey(request);
            if (validationResponse.Code != 200)
            {
                return validationResponse;
            }

            // Update inbound details
            ApplyUpdates(inbound, request);
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

            if (request.DateFrom.HasValue)
            {
                query = query.Where(i => i.CreatedAt >= request.DateFrom.Value);
            }
            if (request.DateTo.HasValue)
            {
                query = query.Where(i => i.CreatedAt <= request.DateTo.Value);
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

        private async Task<BaseResponse> CheckForeignKey<T>(T request) where T : class
        {
            Guid accountId;
            int? providerId = null;
            int? productId = null;
            int? warehouseId = null;
            bool checkProvider = false, checkProduct = false, checkWarehouse = false;

            if (request is CreateInboundRequest createRequest)
            {
                accountId = createRequest.AccountId;
                providerId = createRequest.ProviderId;
                productId = createRequest.ProductId;
                warehouseId = createRequest.WarehouseId;
                checkProvider = checkProduct = checkWarehouse = true; // Always check in CreateInboundRequest
            }
            else if (request is UpdateInboundRequest updateRequest)
            {
                accountId = updateRequest.AccountId;
                if (updateRequest.ProviderId.HasValue)
                {
                    providerId = updateRequest.ProviderId;
                    checkProvider = true;
                }
                if (updateRequest.ProductId.HasValue)
                {
                    productId = updateRequest.ProductId;
                    checkProduct = true;
                }
                if (updateRequest.WarehouseId.HasValue)
                {
                    warehouseId = updateRequest.WarehouseId;
                    checkWarehouse = true;
                }
            }
            else
            {
                return new BaseResponse { Code = 400, Message = "Invalid request type" };
            }

            // Validate if the account exists
            var account = await _unitOfWork.AccountRepository.GetByIdAsync(accountId);
            if (account == null)
            {
                return new BaseResponse { Code = 404, Message = "Account not found" };
            }

            // Validate if the provider exists (only if necessary)
            if (checkProvider && providerId.HasValue)
            {
                var provider = await _unitOfWork.ProviderRepository.GetByIdAsync(providerId.Value);
                if (provider == null)
                {
                    return new BaseResponse { Code = 404, Message = "Provider does not exist" };
                }
            }

            // Validate if the product exists (only if necessary)
            if (checkProduct && productId.HasValue)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId.Value);
                if (product == null)
                {
                    return new BaseResponse { Code = 404, Message = "Product does not exist" };
                }
            }

            // Validate if the warehouse exists (only if necessary)
            if (checkWarehouse && warehouseId.HasValue)
            {
                var warehouse = await _unitOfWork.WarehouseRepository.GetByIdAsync(warehouseId.Value);
                if (warehouse == null)
                {
                    return new BaseResponse { Code = 404, Message = "Warehouse does not exist" };
                }
            }

            return new BaseResponse { Code = 200, Message = "Validation successful" };
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

        // Method to apply updates to an existing inbound entity
        public void ApplyUpdates(Inbound inbound, UpdateInboundRequest request)
        {
            inbound.ProviderOrderCode = string.IsNullOrWhiteSpace(request.ProviderOrderCode) ? inbound.ProviderOrderCode : request.ProviderOrderCode;
            inbound.Note = string.IsNullOrWhiteSpace(request.Note) ? inbound.Note : request.Note;
            inbound.ProviderId = request.ProviderId ?? inbound.ProviderId;
            inbound.Quantity = request.Quantity ?? inbound.Quantity;
            inbound.Price = request.Price ?? inbound.Price;
            inbound.ProductId = request.ProductId ?? inbound.ProductId;
            inbound.WarehouseId = request.WarehouseId ?? inbound.WarehouseId;
            inbound.InboundDate = request.InboundDate ?? inbound.InboundDate;

            inbound.AccountId = request.AccountId;
            inbound.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
        }

    }
}
