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
        public Task<BaseResponse> CreateInboundRequest(CreateInboundRequest request);
        public Task<BaseResponse> UpdateInboundRequest(UpdateInboundRequest request);
        public Task<BaseResponse> UpdateInboundRequestStatus(UpdateInboundStatusRequest request);
        public Task<BaseResponse> DeleteInboundRequest(int inboundId);
        public Task<ViewInbound> GetInboundRequestById(int inboundId);
        public Task<PaginatedResult<ViewInbound>> GetInboundRequestsPaginatedAsync(QueryPaging request);
    }
}
