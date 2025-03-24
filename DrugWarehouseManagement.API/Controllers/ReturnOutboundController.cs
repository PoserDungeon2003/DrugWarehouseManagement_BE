using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _returnOutboundService.GetReturnOutboundByOutboundIdAsync(outboundId);
            return Ok(result);
        }
    }
}
