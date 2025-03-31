using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
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
            var response = new BaseResponse();
            // Check if the provider exists
            var provider = await _unitOfWork.ProviderRepository
                                        .GetByIdAsync(request.ProviderId);

            if (provider == null)
            {
                throw new Exception("Provider not found.");
            }

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
                                .GetAll()
                                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }

            // Check if the provider exists
            var provider = await _unitOfWork.ProviderRepository
                                        .GetAll()
                                        .FirstOrDefaultAsync(p => p.ProviderId == request.ProviderId);
            if (provider == null)
            {
                throw new Exception("Provider not found.");
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
                                .GetAll()
                                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                throw new Exception("Product not found.");
            }
            product.Status = ProductStatus.Inactive;

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Product deleted successfully."
            };
        }
    }
}
