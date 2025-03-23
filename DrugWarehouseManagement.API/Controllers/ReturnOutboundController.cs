using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
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
            await _returnOutboundService.CreateReturnOutboundAsync(request);
            return Ok(new { message = "Return outbound created successfully" });
        }

        [HttpGet("{outboundId}")]
        public async Task<IActionResult> GetReturnOutbound(int outboundId)
        {
            var result = await _returnOutboundService.GetReturnOutboundByOutboundIdAsync(outboundId);
            return Ok(result);
        }
    }
}
