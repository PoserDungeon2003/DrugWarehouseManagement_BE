using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class CategoriesService : ICategoriesService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesService(
            IUnitOfWork unitOfWork
        )
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> ActiveCategory(int categoryId)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new Exception("Category not found");
            }

            if (category.Status == Common.CategoriesStatus.Active)
            {
                throw new Exception("Category is already actived");
            }

            category.Status = Common.CategoriesStatus.Active;

            //await _unitOfWork.CategoriesRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Category actived successfully"
            };
        }

        public async Task<BaseResponse> CreateCategory(CreateCategoryRequest createCategoryRequest)
        {
            var category = createCategoryRequest.Adapt<Categories>();
            await _unitOfWork.CategoriesRepository.CreateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Category created successfully"
            };
        }

        public async Task<BaseResponse> DeleteCategory(int categoryId)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(categoryId);
            if (category == null)
            {
                throw new Exception("Category not found");
            }

            if (category.Status == Common.CategoriesStatus.Inactive)
            {
                throw new Exception("Category is already deleted");
            }

            category.Status = Common.CategoriesStatus.Inactive;

            //await _unitOfWork.CategoriesRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Category deleted successfully"
            };
        }

        public async Task<PaginatedResult<ViewCategories>> GetListCategories(QueryPaging query)
        {
            var categories = _unitOfWork.CategoriesRepository.GetAll()
                                    .Include(c => c.ParentCategory)
                                    .Include(c => c.SubCategories)
                                    .OrderByDescending(lt => lt.UpdatedAt.HasValue)
                                    .ThenByDescending(lt => lt.UpdatedAt)
                                    .ThenByDescending(lt => lt.CreatedAt)
                                    .AsQueryable();

            if (!string.IsNullOrEmpty(query.Search))
            {
                query.Search = query.Search.Trim().ToLower();
                categories = categories
                                 .Where(c => 
                                        c.CategoryName.Contains(query.Search) || 
                                        c.CategoriesId.ToString().Contains(query.Search)
                                 );
            }

            if (query.DateFrom != null)
            {
                var dateFrom = InstantPattern.ExtendedIso.Parse(query.DateFrom);
                if (!dateFrom.Success)
                {
                    throw new Exception("DateFrom is invalid ISO format");
                }
                categories = categories.Where(lt => lt.CreatedAt >= dateFrom.Value);
            }

            if (query.DateTo != null)
            {
                var dateTo = InstantPattern.ExtendedIso.Parse(query.DateTo);
                if (!dateTo.Success)
                {
                    throw new Exception("DateTo is invalid ISO format");
                }
                categories = categories.Where(lt => lt.CreatedAt <= dateTo.Value);
            }

            var result = await categories.ToPaginatedResultAsync(query.Page, query.PageSize);

            return result.Adapt<PaginatedResult<ViewCategories>>();
        }

        public async Task<BaseResponse> UpdateCategory(UpdateCategoryRequest updateCategoryRequest)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByIdAsync(updateCategoryRequest.CategoriesId);

            if (category == null)
            {
                throw new Exception("Category not found");
            }

            updateCategoryRequest.Adapt(category);

            if (updateCategoryRequest.ParentCategoryId != null)
            {
                var parentCategory = await _unitOfWork.CategoriesRepository.GetByIdAsync(updateCategoryRequest.ParentCategoryId);
                if (parentCategory == null)
                {
                    throw new Exception("Parent category not found");
                }
            }
            category.ParentCategoryId = updateCategoryRequest.ParentCategoryId;

            await _unitOfWork.CategoriesRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Category updated successfully",
            };
        }
    }
}
