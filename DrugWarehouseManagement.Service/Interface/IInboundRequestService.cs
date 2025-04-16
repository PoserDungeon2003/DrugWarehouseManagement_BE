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
    public interface IInboundRequestService
    {
        public Task<BaseResponse> CreateInboundRequest(Guid accountId, CreateInboundOrderRequest request);
        public Task<BaseResponse> UpdateInboundRequest(Guid accountId, UpdateInboundOrderRequest request);
        public Task<BaseResponse> UpdateInboundRequestStatus(Guid accountId, UpdateInboundOrderStatusRequest request);
        public Task<ViewInboundRequest> GetInboundRequestById(int inboundRequestId);
        public Task<PaginatedResult<ViewInboundRequest>> GetInboundRequestsPaginatedAsync(InboundRequestQueryPaging request);
    }
}
