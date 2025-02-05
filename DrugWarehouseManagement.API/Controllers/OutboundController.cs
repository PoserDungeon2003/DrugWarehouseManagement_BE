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
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _outboundService.CreateOutbound(accountId, request);
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

        /// <summary>
        /// Searches outbounds by OutboundId or OutboundCode using pagination.
        /// Example: GET api/outbounds/search?page=1&pageSize=10&search=OUTB-1234
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchOutbounds([FromQuery] QueryPaging queryPaging)
        {
            var result = await _outboundService.SearchOutboundsAsync(queryPaging);
            return Ok(result);
        }
    }
}
