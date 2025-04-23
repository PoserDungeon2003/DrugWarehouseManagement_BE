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

        public async Task<BaseResponse> CreateCategory(CreateCategoryRequest createCategoryRequest)
        {
            if (createCategoryRequest.ParentCategoryId.HasValue)
            {
                // Verify parent category exists
                var parentCategory = await _unitOfWork.CategoriesRepository
                    .GetByIdAsync(createCategoryRequest.ParentCategoryId);

                if (parentCategory == null)
                {
                    throw new Exception($"Ko tìm thấy danh mục cha với ID {createCategoryRequest.ParentCategoryId.Value}");
                }

                // Check if main category name already exists
                var existingCategory = await _unitOfWork.CategoriesRepository
                    .AnyAsync(c => c.CategoryName.ToLower().Trim() == createCategoryRequest.CategoryName.ToLower().Trim());

                if (existingCategory)
                {
                    throw new Exception("Tên danh muc đã tồn tại");
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
                string message = "Danh mục con được tạo thành công";
                if (subcategoryResult.NewCount > 0)
                {
                    message += $" với {subcategoryResult.NewCount} danh mục con mới được thêm vào";
                    if (subcategoryResult.ExistingCount > 0)
                    {
                        message += $" và {subcategoryResult.ExistingCount} danh mục con đã tồn tại được liên kết làm con";
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
                    throw new Exception("Tên danh muc đã tồn tại");
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

                string message = "Tạo danh mục thành công";
                if (createCategoryRequest.SubCategories != null && createCategoryRequest.SubCategories.Any())
                {
                    message += $" với {subcategoryResult.NewCount} danh mục con mới";
                    if (subcategoryResult.ExistingCount > 0)
                    {
                        message += $" và {subcategoryResult.ExistingCount} danh mục con đã tồn tại được liên kết làm danh mục con";
                    }
                }
                return new BaseResponse
                {
                    Code = 200,
                    Message = message
                };
            }
        }

        public async Task<ViewCategories> GetCategoryById(int categoryId)
        {
            var category = await _unitOfWork.CategoriesRepository.GetByWhere(c => c.CategoriesId == categoryId)
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync();

            if (category == null)
            {
                throw new Exception("Không tìm thấy danh mục");
            }

            return category.Adapt<ViewCategories>();
        }

        public async Task<PaginatedResult<ViewCategories>> GetListCategories(CategoriesQueryPaging query)
        {
            var categories = _unitOfWork.CategoriesRepository.GetAll()
                                    .Include(c => c.ParentCategory)
                                    .Include(c => c.SubCategories)
                                    .OrderByDescending(lt => lt.UpdatedAt.HasValue)
                                    .ThenByDescending(lt => lt.UpdatedAt)
                                    .ThenByDescending(lt => lt.CreatedAt)
                                    .AsQueryable();

            if (query.IsMainCategory != null)
            {
                if (query.IsMainCategory == true)
                {
                    categories = categories.Where(c => c.ParentCategoryId == null);
                }
                else
                {
                    categories = categories.Where(c => c.ParentCategoryId != null);
                }
            }

            if (!string.IsNullOrEmpty(query.Search))
            {
                query.Search = query.Search.Trim().ToLower();
                categories = categories
                                 .Where(c =>
                                        c.CategoryName.ToLower().Contains(query.Search) ||
                                        c.CategoriesId.ToString().ToLower().Contains(query.Search)
                                 );
            }

            if (query.DateFrom != null)
            {
                var dateFrom = InstantPattern.ExtendedIso.Parse(query.DateFrom);
                if (!dateFrom.Success)
                {
                    throw new Exception("Ngày bắt đầu (DateFrom) không đúng định dạng ISO");
                }
                categories = categories.Where(lt => lt.CreatedAt >= dateFrom.Value);
            }

            if (query.DateTo != null)
            {
                var dateTo = InstantPattern.ExtendedIso.Parse(query.DateTo);
                if (!dateTo.Success)
                {
                    throw new Exception("Ngày kết thúc (DateTo) không đúng định dạng ISO");
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
                throw new Exception("Không tìm thấy danh mục");
            }

            if (updateCategoryRequest.Status == Common.CategoriesStatus.Inactive)
            {
                throw new Exception("Danh mục đã bị xóa");
            }

            if (updateCategoryRequest.Status == Common.CategoriesStatus.Active)
            {
                throw new Exception("Danh mục đã được kích hoạt");
            }

            updateCategoryRequest.Adapt(category);

            if (updateCategoryRequest.ParentCategoryId != null)
            {
                var parentCategory = await _unitOfWork.CategoriesRepository.GetByIdAsync(updateCategoryRequest.ParentCategoryId);
                if (parentCategory == null)
                {
                    throw new Exception("Không tìm thấy danh mục cha");
                }
            }
            //category.ParentCategoryId = updateCategoryRequest.ParentCategoryId;

            await _unitOfWork.CategoriesRepository.UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse
            {
                Code = 200,
                Message = "Cập nhật danh mục thành công",
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
                throw new Exception($"Tên danh mục con bị trùng trong yêu cầu: {string.Join(", ", duplicateSubcategoryNames)}");
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
