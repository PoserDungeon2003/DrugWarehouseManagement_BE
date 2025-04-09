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
            if (createCategoryRequest.ParentCategoryId.HasValue)
            {
                // Verify parent category exists
                var parentCategory = await _unitOfWork.CategoriesRepository
                    .GetByIdAsync(createCategoryRequest.ParentCategoryId);

                if (parentCategory == null)
                {
                    throw new Exception($"Parent category with ID {createCategoryRequest.ParentCategoryId.Value} not found");
                }

                // Check if main category name already exists
                var existingCategory = await _unitOfWork.CategoriesRepository
                    .AnyAsync(c => c.CategoryName.ToLower().Trim() == createCategoryRequest.CategoryName.ToLower().Trim());

                if (existingCategory)
                {
                    throw new Exception("Category name already exists");
                }

                // Create the new subcategory
                var newSubCategory = new Categories
                {
                    CategoryName = createCategoryRequest.CategoryName,
                    Description = createCategoryRequest.Description,
                    ParentCategoryId = createCategoryRequest.ParentCategoryId
                };

                await _unitOfWork.CategoriesRepository.CreateAsync(newSubCategory);
                await _unitOfWork.SaveChangesAsync();

                // Handle nested subcategories (if provided)
                var subcategoryResult = new { ExistingCount = 0, NewCount = 0 };

                if (createCategoryRequest.SubCategories != null && createCategoryRequest.SubCategories.Any())
                {
                    // These will be sub-subcategories (nested one level deeper)
                    subcategoryResult = await ProcessSubcategories(createCategoryRequest.SubCategories, newSubCategory.CategoriesId);
                }

                // Prepare response message
                string message = "Subcategory created successfully";
                if (subcategoryResult.NewCount > 0)
                {
                    message += $" with {subcategoryResult.NewCount} new nested subcategories";
                    if (subcategoryResult.ExistingCount > 0)
                    {
                        message += $" and {subcategoryResult.ExistingCount} existing subcategories linked as children";
                    }
                }

                return new BaseResponse
                {
                    Code = 200,
                    Message = message
                };
            }
            else
            {
                var existingCategory = await _unitOfWork.CategoriesRepository
                .AnyAsync(c => c.CategoryName.ToLower().Trim() == createCategoryRequest.CategoryName.ToLower().Trim());

                if (existingCategory)
                {
                    throw new Exception("Category name already exists");
                }

                // Handle subcategories
                // Create the parent category first
                var parentCategory = new Categories
                {
                    CategoryName = createCategoryRequest.CategoryName,
                    Description = createCategoryRequest.Description,
                };
                await _unitOfWork.CategoriesRepository.CreateAsync(parentCategory);
                await _unitOfWork.SaveChangesAsync(); // Save to generate the ID for the parent category

                var subcategoryResult = new { ExistingCount = 0, NewCount = 0 };

                // Handle subcategories
                if (createCategoryRequest.SubCategories != null && createCategoryRequest.SubCategories.Any())
                {
                    subcategoryResult = await ProcessSubcategories(createCategoryRequest.SubCategories, parentCategory.CategoriesId);
                }

                string message = "Category created successfully";
                if (createCategoryRequest.SubCategories != null && createCategoryRequest.SubCategories.Any())
                {
                    message += $" with {subcategoryResult.NewCount} new subcategories";
                    if (subcategoryResult.ExistingCount > 0)
                    {
                        message += $" and {subcategoryResult.ExistingCount} existing subcategories linked as children";
                    }
                }
                return new BaseResponse
                {
                    Code = 200,
                    Message = message
                };
            }
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

        private async Task<dynamic> ProcessSubcategories(List<CreateSubCategories> subcategories, int parentId)
        {
            int existingCount = 0;
            int newCount = 0;

            // Check for duplicate subcategory names within the request
            var subcategoryNames = subcategories.Select(s => s.CategoryName.ToLower().Trim()).ToList();
            var duplicateSubcategoryNames = subcategoryNames
                .GroupBy(name => name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSubcategoryNames.Any())
            {
                throw new Exception($"Duplicate subcategory name(s) in request: {string.Join(", ", duplicateSubcategoryNames)}");
            }

            // Get existing subcategories from database
            var existingSubcategories = await _unitOfWork.CategoriesRepository
                .GetByWhere(c => subcategoryNames.Contains(c.CategoryName.ToLower().Trim()))
                .ToListAsync();

            var existingSubcategoryNames = existingSubcategories
                .Select(c => c.CategoryName.ToLower().Trim())
                .ToList();

            // For existing subcategories, update their ParentCategoryId
            foreach (var existingSubcategory in existingSubcategories)
            {
                existingSubcategory.ParentCategoryId = parentId; // Set the parent category ID
            }

            if (existingSubcategories.Any())
            {
                await _unitOfWork.CategoriesRepository.UpdateRangeAsync(existingSubcategories);
                existingCount = existingSubcategories.Count;
            }

            // Create new subcategories with ParentCategoryId set
            var newSubcategories = new List<Categories>();
            foreach (var subCategoryRequest in subcategories)
            {
                if (!existingSubcategoryNames.Contains(subCategoryRequest.CategoryName.ToLower().Trim()))
                {
                    var subCategory = new Categories
                    {
                        CategoryName = subCategoryRequest.CategoryName,
                        Description = subCategoryRequest.Description,
                        ParentCategoryId = parentId
                    };
                    newSubcategories.Add(subCategory);
                }
            }

            // Create all new subcategories
            if (newSubcategories.Any())
            {
                await _unitOfWork.CategoriesRepository.AddRangeAsync(newSubcategories);
                newCount = newSubcategories.Count;
            }

            await _unitOfWork.SaveChangesAsync();

            // Return an anonymous object with the counts
            return new { ExistingCount = existingCount, NewCount = newCount };
        }
    }
}
