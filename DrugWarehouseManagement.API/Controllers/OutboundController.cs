using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboundController : ControllerBase
    {
        private readonly IOutboundService _outboundService;

        public OutboundController(IOutboundService outboundService)
        {
            _outboundService = outboundService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOutbound([FromBody] CreateOutboundRequest request)
        {
            var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var response = await _outboundService.CreateOutbound(accountId, request);
            return StatusCode(response.Code, response);
        }
    }
}
