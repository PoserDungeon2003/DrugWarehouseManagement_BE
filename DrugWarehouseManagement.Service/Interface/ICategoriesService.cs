﻿using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface ICategoriesService
    {
        public Task<PaginatedResult<ViewCategories>> GetListCategories(CategoriesQueryPaging query);
        public Task<BaseResponse> CreateCategory(CreateCategoryRequest createCategoryRequest);
        public Task<BaseResponse> UpdateCategory(UpdateCategoryRequest updateCategoryRequest);
        public Task<ViewCategories> GetCategoryById(int categoryId);
    }
}
