using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoriesService;

        public CategoriesController(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetListCategories([FromQuery] CategoriesQueryPaging query)
        {
            try
            {
                var result = await _categoriesService.GetListCategories(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Sale Admin, Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest createCategoryRequest)
        {
            try
            {
                var result = await _categoriesService.CreateCategory(createCategoryRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Sale Admin, Admin")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest updateCategoryRequest)
        {
            try
            {
                var result = await _categoriesService.UpdateCategory(updateCategoryRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetCategoryById([FromRoute] int id)
        {
            try
            {
                var result = await _categoriesService.GetCategoryById(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message
                });
            }
        }
    }
}
