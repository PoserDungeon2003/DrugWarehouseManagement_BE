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
    public interface IInboundReportService
    {
        public Task<BaseResponse> CreateInboundReport(CreateInboundReportRequest request);
        public Task<BaseResponse> UpdateInboundReport(UpdateInboundReportRequest request);
        public Task<BaseResponse> DeleteInboundReport(int inboundReporttId);
        public Task<ViewInbound> GetInboundReportById(int inboundReporttId);
        public Task<PaginatedResult<ViewInbound>> GetInboundReportsPaginatedAsync(QueryPaging request);
    }
}
