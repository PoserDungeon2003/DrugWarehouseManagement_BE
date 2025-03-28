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
    public interface IDeviceService
    {
        public Task<BaseResponse> RegisterDevice(Guid accountId, RegisterDeviceRequest request);
        public Task<PaginatedResult<ViewDevices>> GetDevices(QueryPaging queryPaging);
        public Task<BaseResponse> Ping(string apiKey);
        public Task<BaseResponse> UpdateTrackingNumber(string apiKey, UpdateTrackingNumberRequest request);
        public Task<BaseResponse> UpdateDevice(Guid accountId, UpdateDeviceRequest request);
    }
}
