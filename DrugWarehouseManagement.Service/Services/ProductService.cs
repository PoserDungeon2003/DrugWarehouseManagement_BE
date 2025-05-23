﻿using DrugWarehouseManagement.Common;
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
            var existedProduct = await _unitOfWork.ProductRepository
                    .GetByWhere(p => p.ProductName == request.ProductName)
                    .FirstOrDefaultAsync();
            if (existedProduct != null && existedProduct.ProductName == request.ProductName)
            {
                throw new Exception("Tên sản phẩm này đã tồn tại.");
            }
            // Map the DTO to the Product entity
            var product = request.Adapt<Product>();
            // Add the product via the repository
            await _unitOfWork.ProductRepository.CreateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Tạo sản phẩm thành công."
            };
        }

        public async Task<PaginatedResult<ProductResponse>> GetProductsAsync(GetProductRequest request)
        {
            var query = _unitOfWork.ProductRepository
                        .GetAll()
                        .Include(pc => pc.Categories)
                        .AsQueryable();

            if (!request.ShowInactive)
            {
                query = query.Where(p => p.Status == ProductStatus.Active);
            }
            
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
                    throw new Exception("Trạng thái không hợp lệ");
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
                                .FirstOrDefaultAsync();

            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }

            // Update product properties
            request.Adapt(product);
            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            // Now handle categories separately with explicit SQL operations if needed
            if (request.ProductCategories != null && request.ProductCategories.Any())
            {
                // Get all existing product-category relationships
                var existingRelations = await _unitOfWork.ProductCategoriesRepository
                    .GetByWhere(pc => pc.ProductId == productId)
                    .ToListAsync();

                // Extract requested category IDs
                var requestedCategoryIds = request.ProductCategories
                    .Where(c => c != null)
                    .Select(c => c.CategoriesId)
                    .Distinct()
                    .ToList();

                // Find categories to remove
                var categoriesToRemove = existingRelations
                    .Where(pc => !requestedCategoryIds.Contains(pc.CategoriesId))
                    .ToList();

                // Remove categories that are not in the request
                if (categoriesToRemove.Any())
                {
                    await _unitOfWork.ProductCategoriesRepository.DeleteRangeAsync(categoriesToRemove);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Find categories to add
                var existingCategoryIds = existingRelations.Select(pc => pc.CategoriesId).ToHashSet();
                var categoriesToAdd = requestedCategoryIds
                    .Where(id => !existingCategoryIds.Contains(id))
                    .Select(catId => new ProductCategories
                    {
                        ProductId = productId,
                        CategoriesId = catId
                    })
                    .ToList();

                // Add new categories
                if (categoriesToAdd.Any())
                {
                    await _unitOfWork.ProductCategoriesRepository.AddRangeAsync(categoriesToAdd);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            else if (request.ProductCategories != null) // Empty list provided - remove all categories
            {
                var existingRelations = await _unitOfWork.ProductCategoriesRepository
                    .GetByWhere(pc => pc.ProductId == productId)
                    .ToListAsync();

                if (existingRelations.Any())
                {
                    await _unitOfWork.ProductCategoriesRepository.DeleteRangeAsync(existingRelations);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Cập nhật sản phẩm thành công."
            };
        }

        public async Task<BaseResponse> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.ProductRepository
                                .GetByIdAsync(productId);

            if (product == null)
            {
                throw new Exception("Không tìm thấy sản phẩm.");
            }
            product.Status = ProductStatus.Inactive;

            await _unitOfWork.ProductRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Xóa sản phầm thành công."
            };
        }
    }
}
