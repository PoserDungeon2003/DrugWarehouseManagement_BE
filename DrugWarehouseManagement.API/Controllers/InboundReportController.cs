using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCvSharp;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundReportController : ControllerBase
    {
        private readonly IInboundReportService _inboundReportService;

        public InboundReportController(IInboundReportService inboundReportService)
        {
            _inboundReportService = inboundReportService;
        }

        [HttpPost]
        [Authorize(Roles = "Sale Admin, Accountant, Inventory Manager")]
        public async Task<IActionResult> CreateInboundReport([FromForm] CreateInboundReportRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundReportService.CreateInboundReport(accountId, request);
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
        [Authorize(Roles = "Sale Admin, Accountant, Inventory Manager")]
        public async Task<IActionResult> UpdateInboundReport([FromForm] UpdateInboundReportRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inboundReportService.UpdateInboundReport(accountId, request);
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
    }
}
