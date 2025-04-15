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
    public interface IInventoryCheckService
    {
        public Task<BaseResponse> CreateInventoryCheck(Guid accountId, CreateInventoryCheckRequest request);
        public Task<byte[]> GenerateInventoryCheckPdfAsync(int inboundId);
        public Task<PaginatedResult<ViewInventoryCheck>> GetInventoryChecksPaginatedAsync(QueryPaging request);
    }
}
