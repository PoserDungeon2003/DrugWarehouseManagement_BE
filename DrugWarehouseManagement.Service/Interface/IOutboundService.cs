using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IOutboundService
    {
        public Task<BaseResponse> UpdateOutbound(int outboundId, UpdateOutboundRequest request);
        public Task<BaseResponse> CreateOutbound(Guid accountId, CreateOutboundRequest request);
        Task<PaginatedResult<OutboundResponse>> SearchOutboundsAsync(QueryPaging queryPaging);
        Task<Outbound?> GetOutboundByIdWithDetailsAsync(int outboundId);

    }
}
