using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        // POST: api/Warehouse
        [HttpPost]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseRequest request)
        {
            try
            {
                await _warehouseService.CreateWarehouseAsync(request);
                return Ok(new BaseResponse
                {
                    Code = 200,
                    Message = "Warehouse created successfully."
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

        // GET: api/Warehouse/search?page=1&pageSize=10&search=...
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchWarehouse([FromQuery] SearchWarehouseRequest request)
        {
            try
            {
                var result = await _warehouseService.SearchWarehousesAsync(request);
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

        // PUT: api/Warehouse/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] UpdateWarehouseRequest request)
        {
            try
            {
                await _warehouseService.UpdateWarehouseAsync(id, request);
                return Ok(new BaseResponse
                {
                    Code = 200,
                    Message = "Warehouse updated successfully."
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
        // DELETE: api/Warehouse/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            try
            {
                var response = await _warehouseService.DeleteWarehouseAsync(id);
                return Ok(response);
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
