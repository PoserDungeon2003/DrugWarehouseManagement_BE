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
    public interface IInboundService
    {
        public Task<BaseResponse> CreateInbound(CreateInboundRequest request);
        public Task<BaseResponse> CreateInboundDetail(CreateInboundRequest request);
        public Task<BaseResponse> UpdateInbound(UpdateInboundRequest request);
        public Task<BaseResponse> UpdateInboundStatus(UpdateInboundStatusRequest request);
        public Task<BaseResponse> DeleteInbound(int inboundId);
        public Task<ViewInbound> GetInboundById(int inboundId);
        public Task<PaginatedResult<ViewInbound>> GetInboundsPaginatedAsync(QueryPaging request);
    }
}
