    using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
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
        [Authorize(Roles = "Inventory Manager, Sale Admin")]
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

        [HttpPost("sample-export")]
        [Authorize(Roles ="Inventory Manager, Sale Admin")]
        public async Task<IActionResult> CreateSampleOutbound([FromBody] CreateOutboundRequest request)
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
        [HttpPut]
        [Authorize(Roles = "Inventory Manager, Sale Admin")]
        public async Task<IActionResult> UpdateOutbound(int id, [FromBody] UpdateOutboundRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _outboundService.UpdateOutbound(id, request);
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
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchOutbounds([FromQuery] SearchOutboundRequest request)
        {
            try
            {
                var result = await _outboundService.SearchOutboundsAsync(request);
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
        
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOutboundById(int id)
        {
            try
            {
                var outbound = await _outboundService.GetOutboundByIdAsync(id);
                if (outbound == null)
                {
                    return NotFound(new BaseResponse
                    {
                        Code = 404,
                        Message = "Outbound not found."
                    });
                }
                return Ok(outbound);

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
        [HttpGet("export/{id}")]
        public async Task<IActionResult> ExportOutboundInvoice(int id)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var pdfBytes = await _outboundService.GenerateOutboundInvoicePdfAsync(id);
                // Fetch code again for naming
                var outbound = await _outboundService.GetOutboundByIdWithDetailsAsync(id);
                if (outbound == null)
                    return NotFound(new BaseResponse { Code = 404, Message = "Outbound not found." });

                var fileName = $"PhieuGiaoNhan_{outbound.OutboundCode}.pdf";

                return File(pdfBytes,
                            "application/pdf",
                            fileName);
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
