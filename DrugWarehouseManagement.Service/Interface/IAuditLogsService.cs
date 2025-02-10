using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IAuditLogsService
    {
        public Task<PaginatedResult<ViewAuditLogs>> ViewLogsAsync(QueryPaging queryPaging);
    }
}
