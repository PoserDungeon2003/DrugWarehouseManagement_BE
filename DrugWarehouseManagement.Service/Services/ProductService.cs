using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio.DataModel;
using System.Net;

namespace DrugWarehouseManagement.Service.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateProductAsync(CreateProductRequest request)
        {            
            // Map the DTO to the Product entity
            var product = request.Adapt<Product>();
            // Add the product via the repository
            await _unitOfWork.ProductRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Product created successfully."
            };
        }

        public async Task<PaginatedResult<ProductResponse>> GetProductsAsync(GetProductRequest request)
        {
            var query = _unitOfWork.ProductRepository
                        .GetAll()
                        .Where(p => p.Status == ProductStatus.Active)
                        .Include(pc => pc.Categories)
                        .AsQueryable();
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.Categories.Any(c => c.CategoriesId == request.CategoryId.Value));
            }
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ProductStatus>(request.Status, true, out var parsedStatus))
                {
                    query = query.Where(o => o.Status == parsedStatus);
                }
                else
                {
                    throw new Exception("Status is invalid.");
                }
            }

            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchTerm = request.Search.Trim().ToLower();
                query = query.Where(p =>
                    EF.Functions.Like(p.ProductName.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(p.ProductCode.ToLower(), $"%{searchTerm}%") ||
                    EF.Functions.Like(p.SKU.ToLower(), $"%{searchTerm}%")
                );
            }

            query = query.OrderByDescending(p => p.ProductId);
            var paginatedProducts = await query.ToPaginatedResultAsync(request.Page, request.PageSize);
            var response = paginatedProducts.Adapt<PaginatedResult<ProductResponse>>();

            return response;
        }

        public async Task<BaseResponse> UpdateProductAsync(int productId, UpdateProductRequest request)
        {
            var product = await _unitOfWork.ProductRepository
                                .GetByWhere(p => p.ProductId == productId)
                                .Include(pc => pc.ProductCategories)
                                .Include(c => c.Categories)
                                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            if (request.ProductCategories != null)
            {
                var requestedCategoryIds = new HashSet<int>(request.ProductCategories.Select(pc => pc.CategoriesId));

                var categoriesToDelete = product.ProductCategories
                    .Where(category => !requestedCategoryIds.Contains(category.CategoriesId))
                    .ToList();

                await _unitOfWork.ProductCategoriesRepository.DeleteRangeAsync(categoriesToDelete);
            }
            request.Adapt(product);

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Product updated successfully."
            };
        }

        public async Task<BaseResponse> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.ProductRepository
                                .GetByIdAsync(productId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }
            product.Status = ProductStatus.Inactive;

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Product deleted successfully."
            };
        }
    }
}
