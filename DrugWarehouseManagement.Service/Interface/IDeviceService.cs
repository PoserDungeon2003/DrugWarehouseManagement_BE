using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IDeviceService
    {
        public Task<BaseResponse> RegisterDevice(Guid accountId, RegisterDeviceRequest request);
        public Task<BaseResponse> Ping(string apiKey);
    }
}
