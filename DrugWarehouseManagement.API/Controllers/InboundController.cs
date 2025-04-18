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
    public class InboundController : ControllerBase
    {
        private readonly IInboundService _inboundService;
        private readonly IInboundReportService _inboundReportService;

        public InboundController(IInboundService inboundService, IInboundReportService inboundReportService)
        {
            _inboundService = inboundService;
            _inboundReportService = inboundReportService;
        }

        [HttpPost]
        [Authorize(Roles = "Sale Admin, Inventory Manager")]
        public async Task<IActionResult> CreateInbound([FromBody] CreateInboundRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundService.CreateInbound(accountId, request);
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
        [Authorize(Roles = "Sale Admin, Inventory Manager")]
        public async Task<IActionResult> UpdateInbound([FromBody] UpdateInboundRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundService.UpdateInbound(accountId, request);
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
        [Authorize(Roles = "Sale Admin, Inventory Manager")]
        public async Task<IActionResult> UpdateInboundStatus([FromBody] UpdateInboundStatusRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundService.UpdateInboundStatus(accountId, request);
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
        [Authorize]
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetInboundsPaginated([FromQuery] InboundtQueryPaging request)
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

        [HttpGet("{inboundId}/pdf")]
        [Authorize]
        public async Task<IActionResult> GetInboundPdf(int inboundId)
        {
            try
            {
                // Gọi service để tạo PDF
                byte[] pdfBytes = await _inboundService.GenerateInboundPdfAsync(inboundId);

                // Trả về file PDF
                return File(pdfBytes, "application/pdf", $"inbound_{inboundId}.pdf");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu inbound không tồn tại hoặc có lỗi khi tạo PDF
                return NotFound(new BaseResponse
                {
                    Code = 404,
                    Message = ex.Message ?? "Inbound not found or failed to generate PDF"
                });
            }
        }

        [HttpGet("{inboundId}/inbound-report")]
        [Authorize]
        public async Task<IActionResult> GetInboundReportById(int inboundId)
        {
            try
            {
                var response = await _inboundReportService.GetInboundReportByInboundId(inboundId);
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
    }
}
