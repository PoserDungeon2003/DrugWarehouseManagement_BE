using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IProviderService 
    {
        Task<BaseResponse> CreateProviderAsync(CreateProviderRequest request);
        Task<PaginatedResult<ProviderResponse>> SearchProvidersAsync(QueryPaging queryPaging);
        Task<BaseResponse> UpdateProviderAsync(int providerId, UpdateProviderRequest request);
        Task<BaseResponse> DeleteProviderAsync(int providerId);
        Task<ProviderResponse> GetProviderByIdAsync(int providerId);
    }
}
