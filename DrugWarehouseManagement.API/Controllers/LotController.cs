using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LotController : ControllerBase
    {
        private readonly ILotService _lotService;

        public LotController(ILotService lotService)
        {
            _lotService = lotService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateLot([FromBody] CreateLotRequest request)
        {
            try
            {
                var response = await _lotService.CreateLot(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message,
                });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateLot([FromBody] UpdateLotRequest request)
        {
            try
            {
                var response = await _lotService.UpdateLot(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message,
                });
            }
        }

        [HttpDelete("{lotId}")]
        public async Task<IActionResult> DeleteLot(int lotId)
        {
            try
            {
                var response = await _lotService.DeleteLot(lotId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message,
                });
            }
        }

        [HttpGet("{lotId}")]
        public async Task<IActionResult> GetLotById(int lotId)
        {
            try
            {
                var response = await _lotService.GetLotById(lotId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new BaseResponse
                {
                    Code = 404,
                    Message = ex.Message,
                });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetLotsPaginated([FromQuery] QueryPaging request)
        {
            try
            {
                var result = await _lotService.GetLotsPaginatedAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message,
                });
            }
        }
    }

}
