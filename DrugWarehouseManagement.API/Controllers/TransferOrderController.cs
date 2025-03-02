using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferOrderController : ControllerBase
    {
        private readonly ITransferOrderService _transferOrderService;

        public TransferOrderController(ITransferOrderService transferOrderService)
        {
            _transferOrderService = transferOrderService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTransferOrder([FromBody] TransferOrderRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _transferOrderService.CreateTransferOrder(Guid.Parse(accountId), request);
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

        [HttpGet("export/{id}")]
        [Authorize]
        public async Task<IActionResult> ExportTransferOrder([FromRoute] int id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _transferOrderService.ExportTransferOrder(Guid.Parse(accountId), id);
                return File(response, "application/pdf", "PhieuChuyenKho.pdf");
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
