using DrugWarehouseManagement.Common;
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
    public class LotTransferController : ControllerBase
    {
        private readonly ILotTransferService _lotTransferService;

        public LotTransferController(ILotTransferService lotTransferService)
        {
            _lotTransferService = lotTransferService;
        }

        [HttpPost]
        [Authorize(Roles = "Sale Admin, Inventory Manager")]
        public async Task<IActionResult> CreateLotTransfer([FromBody] LotTransferRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _lotTransferService.CreateLotTransfer(Guid.Parse(accountId), request);
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

        [HttpPost("export/{id}")]
        [Authorize]
        public async Task<IActionResult> ExportLotTransfer([FromRoute] int id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _lotTransferService.ExportLotTransfer(Guid.Parse(accountId), id);
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

        [HttpPost("cancel/{id}")]
        [Authorize(Roles = "Sale Admin, Inventory Manager")]
        public async Task<IActionResult> CancelLotTransfer([FromRoute] int id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _lotTransferService.CancelLotTransfer(Guid.Parse(accountId), id);
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLotTransfers([FromQuery] LotTransferQueryPaging request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _lotTransferService.GetLotTransfers(request);
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

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetLotTransferById([FromRoute] int id)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _lotTransferService.GetLotTransferById(id);
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
        public async Task<IActionResult> UpdateLotTransfer([FromBody] UpdateLotTransferRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                var response = await _lotTransferService.UpdateLotTransfer(Guid.Parse(accountId), request);
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
