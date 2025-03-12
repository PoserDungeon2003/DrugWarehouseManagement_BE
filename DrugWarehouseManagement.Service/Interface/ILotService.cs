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
    public interface ILotService
    {
        public Task<BaseResponse> CreateLot(CreateLotRequest request);
        public Task<BaseResponse> UpdateLot(UpdateLotRequest request);
        public Task<BaseResponse> DeleteLot(int lotId);
        public Task<ViewLot> GetLotById(int lotId);
        public Task<PaginatedResult<ViewLot>> GetLotsPaginatedAsync(QueryPaging request);
    }
}
