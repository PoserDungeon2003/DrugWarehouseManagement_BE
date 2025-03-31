using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IProductService
    {
        Task<BaseResponse> CreateProductAsync(CreateProductRequest request);
        Task<PaginatedResult<ProductResponse>> SearchProductsAsync(SearchProductRequest queryPaging);
        Task<BaseResponse> UpdateProductAsync(int productId, UpdateProductRequest request);
        Task<BaseResponse> DeleteProductAsync(int productId);
    }
}
