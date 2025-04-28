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
                                        .GetByWhere(p => p.PhoneNumber == request.PhoneNumber)
                                        .FirstOrDefaultAsync();
            if (provider != null)
            {
                throw new Exception("Nhà cung cấp này đã tồn tại");
            }

            var existedDocumentNumber = await _unitOfWork.ProviderRepository
                .GetByWhere(p => p.DocumentNumber == request.DocumentNumber)
                .FirstOrDefaultAsync();

            if(existedDocumentNumber != null && existedDocumentNumber.DocumentNumber == request.DocumentNumber)
            {
                throw new Exception("Số chứng từ này đã tồn tại.");
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
                Message = "Tạo nhà cung cấp thành công."
            };
        }

        public async Task<BaseResponse> DeleteProviderAsync(int providerId)
        {
            var provider = await _unitOfWork.ProviderRepository
                  .GetByWhere(p => p.ProviderId == providerId)
                  .FirstOrDefaultAsync();
            if (provider == null)
            {
                throw new Exception("Không tìm thấy nhà cung cấp hoặc nhà cung cấp đã bị xóa trước đó.");
            }
            provider.Status = ProviderStatus.Deleted;
            await _unitOfWork.ProviderRepository.UpdateAsync(provider);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Xóa nhà cung cấp thành công."
            };
        }

        public async Task<ProviderResponse> GetProviderByIdAsync(int providerId)
        {
            var provider = await _unitOfWork.ProviderRepository
                 .GetByWhere(x => x.ProviderId == providerId).FirstOrDefaultAsync();
            if (provider == null)
            {
                throw new Exception("Không tìm thấy nhà cung cấp.");
            }
            return provider.Adapt<ProviderResponse>();
        }

        public async Task<PaginatedResult<ProviderResponse>> SearchProvidersAsync(QueryPaging queryPaging)
        {
            var query = _unitOfWork.ProviderRepository
                .GetByWhere(x => x.Status == ProviderStatus.Active)
                .AsQueryable();

            if (!queryPaging.ShowInactive)
            {
                query = query.Where(x => x.Status == ProviderStatus.Active);
            }
            
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
                 .GetByWhere(p => p.ProviderId == providerId)
                 .FirstOrDefaultAsync();

            if (provider == null)
            {
                throw new Exception("Không tìm thấy nhà cung cấp.");
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ProviderStatus>(request.Status, true, out var parsedStatus))
                {
                    provider.Status = parsedStatus;
                }
                else
                {
                    throw new Exception("Trạng thái không hợp lệ.");
                }
            }
            request.Adapt(provider);
            provider.UpdatedAt = NodaTime.SystemClock.Instance.GetCurrentInstant();
            await _unitOfWork.ProviderRepository.UpdateAsync(provider);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Cập nhật thành công."
            };
        }
    }
}
