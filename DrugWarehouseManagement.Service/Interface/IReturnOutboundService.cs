using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IReturnOutboundService
    {
        Task CreateReturnOutboundAsync(CreateReturnOutboundRequest request);
        Task<List<ReturnOutboundDetailsResponse>> GetReturnOutboundByOutboundIdAsync(int outboundId);
        Task<List<ReturnOutboundDetailsResponse>> GetAllReturnOutboundDetailsAsync();
    }
}
