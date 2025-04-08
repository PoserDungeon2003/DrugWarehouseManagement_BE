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
        public Task<BaseResponse> CreateInboundReport(Guid accountid, CreateInboundReportRequest request);
        public Task<BaseResponse> UpdateInboundReport(Guid accountid, UpdateInboundReportRequest request);
        public Task<List<ViewInboundReport>> GetInboundReportByInboundId(int inboundReporttId);
    }
}
