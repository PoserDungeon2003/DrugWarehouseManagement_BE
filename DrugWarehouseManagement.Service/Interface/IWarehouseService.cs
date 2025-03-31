using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IWarehouseService
    {
        Task CreateWarehouseAsync(CreateWarehouseRequest request);
        Task<PaginatedResult<WarehouseResponse>> SearchWarehousesAsync(SearchWarehouseRequest queryPaging);
        Task UpdateWarehouseAsync(int warehouseId, UpdateWarehouseRequest request);
        Task<BaseResponse> DeleteWarehouseAsync(int warehouseId);
    }
}
