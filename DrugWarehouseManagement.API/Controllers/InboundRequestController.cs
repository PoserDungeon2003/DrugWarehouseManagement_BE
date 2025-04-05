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
    public class InboundRequestController : ControllerBase
    {
        private readonly IInboundRequestService _inboundRequestService;

        public InboundRequestController(IInboundRequestService inboundRequestService)
        {
            _inboundRequestService = inboundRequestService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateInboundRequest([FromForm] CreateInboundOrderRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (request.InboundRequestDetails == null || !request.InboundRequestDetails.Any())
                {
                    return BadRequest(new { Code = 400, Message = "InboundRequestDetails cannot be empty." });
                }

                var response = await _inboundRequestService.CreateInboundRequest(accountId, request);
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
        [Authorize]
        public async Task<IActionResult> UpdateInboundRequest([FromForm] UpdateInboundOrderRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundRequestService.UpdateInboundRequest(accountId, request);
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

        [HttpPut("status")]
        [Authorize]
        public async Task<IActionResult> UpdateInboundRequestStatus([FromBody] UpdateInboundOrderStatusRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundRequestService.UpdateInboundRequestStatus(accountId, request);
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
        public async Task<IActionResult> GetInboundRequestById(int inboundId)
        {
            try
            {
                var response = await _inboundRequestService.GetInboundRequestById(inboundId);
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

        [HttpGet]
        public async Task<IActionResult> GetInboundRequestsPaginated([FromQuery] QueryPaging request)
        {
            try
            {
                var result = await _inboundRequestService.GetInboundRequestsPaginatedAsync(request);
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
