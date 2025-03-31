using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace DrugWarehouseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReturnOutboundController : ControllerBase
    {
        private readonly IReturnOutboundService _returnOutboundService;
        public ReturnOutboundController(IReturnOutboundService returnOutboundService)
        {
            _returnOutboundService = returnOutboundService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateReturnOutbound([FromBody] CreateReturnOutboundRequest request)
        {
            try
            {
                await _returnOutboundService.CreateReturnOutboundAsync(request);
                return Ok(new BaseResponse
                {
                    Code = 200,
                    Message = "Return Outbound created successfully."
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

        [HttpGet("{outboundId}")]
        public async Task<IActionResult> GetReturnOutbound(int outboundId)
        {
            var returnOutbound = await _returnOutboundService.GetReturnOutboundByOutboundIdAsync(outboundId);
            if (returnOutbound == null)
            {
                return NotFound(new BaseResponse
                {
                    Code = 404,
                    Message = "Return Outbound not found."
                });
            }
            return Ok(returnOutbound);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReturnOutbound()
        {
            var returnOutbound = await _returnOutboundService.GetAllReturnOutboundDetailsAsync();
            return Ok(returnOutbound);
        }
    }
}
