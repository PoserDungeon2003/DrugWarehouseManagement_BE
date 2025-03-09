using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundController : ControllerBase
    {
        private readonly IInboundService _inboundService;

        public InboundController(IInboundService inboundService)
        {
            _inboundService = inboundService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInbound([FromBody] CreateInboundRequest request)
        {
            try
            {
                var response = await _inboundService.CreateInbound(request);
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
        public async Task<IActionResult> UpdateInbound([FromBody] UpdateInboundRequest request)
        {
            try
            {
                var response = await _inboundService.UpdateInbound(request);
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

        [HttpDelete("{inboundId}")]
        public async Task<IActionResult> DeleteInbound(int inboundId)
        {
            try
            {
                var response = await _inboundService.DeleteInbound(inboundId);
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

        [HttpGet("{inboundId}")]
        public async Task<IActionResult> GetInboundById(int inboundId)
        {
            try
            {
                var response = await _inboundService.GetInboundById(inboundId);
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
        public async Task<IActionResult> GetInboundsPaginated([FromQuery] QueryPaging request)
        {
            try
            {
                var result = await _inboundService.GetInboundsPaginatedAsync(request);
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
