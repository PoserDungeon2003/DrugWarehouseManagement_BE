using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // POST: api/Product
        [HttpPost]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _productService.CreateProductAsync(request);
                return Ok(new BaseResponse
                {
                    Code = 200,
                    Message = "Product created successfully."
                });
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

        // GET: api/Product/search?page=1&pageSize=10&search=...
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts([FromQuery] GetProductRequest request)
        {
            try
            {
                var result = await _productService.GetProductsAsync(request);
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

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                await _productService.UpdateProductAsync(id, request);
                return Ok(new BaseResponse
                {
                    Code = 200,
                    Message = "Product updated successfully."
                });
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

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok(new BaseResponse
                {
                    Code = 200,
                    Message = "Product deleted successfully."
                });
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
