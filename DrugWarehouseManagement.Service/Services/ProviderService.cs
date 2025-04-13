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
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProviderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<BaseResponse> CreateProviderAsync(CreateProviderRequest request)
        {
            var response = new BaseResponse();
            // Check if the provider exists
            var provider = await _unitOfWork.ProviderRepository
                                        .GetAll()
                                        .FirstOrDefaultAsync(p => p.PhoneNumber == request.PhoneNumber);
            if (provider != null)
            {
                throw new Exception("Provider already exist.");
            }

            if(provider.DocumentNumber == request.DocumentNumber)
            {
                throw new Exception("Document number already exist.");
            }
            // Map the DTO to the Provider entity
            provider = request.Adapt<Provider>();
            provider.CreatedAt = SystemClock.Instance.GetCurrentInstant();
            provider.Status = ProviderStatus.Active;
            // Add the provider via the repository
            await _unitOfWork.ProviderRepository.CreateAsync(provider);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Provider created successfully."
            };
        }

        public async Task<BaseResponse> DeleteProviderAsync(int providerId)
        {
            var provider = await _unitOfWork.ProviderRepository
                  .GetAll()
                  .FirstOrDefaultAsync(p => p.ProviderId == providerId);
            if (provider == null)
            {
                throw new Exception("Provider not found.");
            }
            provider.Status = ProviderStatus.Deleted;
            await _unitOfWork.ProviderRepository.UpdateAsync(provider);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Provider deleted successfully."
            };
        }

        public async Task<ProviderResponse> GetProviderByIdAsync(int providerId)
        {
            var provider = await _unitOfWork.ProviderRepository
                 .GetByWhere(x => x.ProviderId == providerId).FirstOrDefaultAsync();
            if (provider == null)
            {
                throw new Exception("Provider not found.");
            }
            return provider.Adapt<ProviderResponse>();
        }

        public async Task<PaginatedResult<ProviderResponse>> SearchProvidersAsync(QueryPaging queryPaging)
        {
            var query = _unitOfWork.ProviderRepository
                .GetByWhere(x => x.Status == ProviderStatus.Active)
                .AsQueryable();
            if (!string.IsNullOrEmpty(queryPaging.Search))
            {
                var searchTerm = queryPaging.Search.Trim().ToLower();
                query = query.Where(p =>
                    EF.Functions.Like(p.ProviderName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(p.PhoneNumber.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(p.Email.ToLower(), $"%{searchTerm}%")
                );
            }
            if (!string.IsNullOrEmpty(queryPaging.DateFrom))
            {
                if (InstantPattern.General.Parse(queryPaging.DateFrom).Success)
                {
                    var fromDate = InstantPattern.General.Parse(queryPaging.DateFrom).Value;
                    query = query.Where(p => p.CreatedAt >= fromDate);
                }
            }
            if (!string.IsNullOrEmpty(queryPaging.DateTo))
            {
                if (InstantPattern.General.Parse(queryPaging.DateTo).Success)
                {
                    var toDate = InstantPattern.General.Parse(queryPaging.DateTo).Value;
                    query = query.Where(p => p.CreatedAt <= toDate);
                }
            }
            query = query.OrderByDescending(p => p.CreatedAt);
            var paginatedProviders = await query.ToPaginatedResultAsync(queryPaging.Page, queryPaging.PageSize);
            var providerResponses = paginatedProviders.Items.Adapt<List<ProviderResponse>>();
            return new PaginatedResult<ProviderResponse>
            {
                Items = providerResponses,
                TotalCount = paginatedProviders.TotalCount,
                PageSize = paginatedProviders.PageSize,
                CurrentPage = paginatedProviders.CurrentPage
            };
        }

        public async Task<BaseResponse> UpdateProviderAsync(int providerId, UpdateProviderRequest request)
        {
            var provider = await _unitOfWork.ProviderRepository
                 .GetAll()
                 .FirstOrDefaultAsync(p => p.ProviderId == providerId);

            if (provider == null )
            {
                throw new Exception("Provider not found.");
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ProviderStatus>(request.Status, true, out var parsedStatus))
                {
                    provider.Status = parsedStatus;
                }
                else
                {
                    throw new Exception("Status is invalid.");
                }
            }
            request.Adapt(provider);
            provider.UpdatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.ProviderRepository.UpdateAsync(provider);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Provider updated successfully."
            };
        }
    }
}
