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
    public class InventoryCheckController : ControllerBase
    {
        private readonly IInventoryCheckService _inventoryCheckService;

        public InventoryCheckController(IInventoryCheckService inventoryCheckService)
        {
            _inventoryCheckService = inventoryCheckService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateInventoryCheck([FromBody] CreateInventoryCheckRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _inventoryCheckService.CreateInventoryCheck(accountId, request);
                return response.Code switch
                {
                    200 => Ok(response),
                    400 => BadRequest(response),
                    404 => NotFound(response),
                    _ => StatusCode(500, response)
                };
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

        [HttpGet("{inventoryCheckId}/pdf")]
        public async Task<IActionResult> GetInventoryCheckPdf(int inventoryCheckId)
        {
            try
            {
                byte[] pdfBytes = await _inventoryCheckService.GenerateInventoryCheckPdfAsync(inventoryCheckId);
                return File(pdfBytes, "application/pdf", $"inventory_check_{inventoryCheckId}.pdf");
            }
            catch (Exception ex)
            {
                return NotFound(new BaseResponse
                {
                    Code = 404,
                    Message = ex.Message ?? "Inventory check not found or failed to generate PDF"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryChecksPaginated([FromQuery] QueryPaging request)
        {
            try
            {
                var result = await _inventoryCheckService.GetInventoryChecksPaginatedAsync(request);
                return Ok(result);
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
