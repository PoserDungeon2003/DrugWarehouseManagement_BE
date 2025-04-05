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
        public Task<BaseResponse> CreateInbound(Guid accountId, CreateInboundRequest request);
        public Task<BaseResponse> UpdateInbound(Guid accountId, UpdateInboundRequest request);
        public Task<BaseResponse> UpdateInboundStatus(Guid accountId, UpdateInboundStatusRequest request);
        public Task<ViewInbound> GetInboundById(int inboundId);
        public Task<byte[]> GenerateInboundPdfAsync(int inboundId);
        public Task<PaginatedResult<ViewInbound>> GetInboundsPaginatedAsync(QueryPaging request);
    }
}
