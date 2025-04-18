using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime.Text;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryReportController : ControllerBase
    {
        private readonly IInventoryReportService _reportService;
        public InventoryReportController(IInventoryReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet("export")]
        [Authorize]
        public async Task <IActionResult> ExportInventoryReportPdf(
           [FromQuery] int warehouseId,
           [FromQuery] string from,
           [FromQuery] string to)
        {
            try
            {
                // 1) Parse 'from' and 'to' as Instant (ISO-8601 strings)
                var parseFrom = InstantPattern.ExtendedIso.Parse(from);
                var parseTo = InstantPattern.ExtendedIso.Parse(to);

                if (!parseFrom.Success || !parseTo.Success)
                {
                    return BadRequest(new BaseResponse
                    {
                        Message = "Invalid date format. Please use ISO-8601 strings, e.g. 2025-01-01T00:00:00Z"
                    });
                }
                var startDate = parseFrom.Value;
                var endDate = parseTo.Value;
                var pdfBytes = await _reportService.ExportInventoryReportPdf(warehouseId, startDate, endDate);
                return File(pdfBytes, "application/pdf", $"BaoCaoNhapXuatTon_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
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

        [HttpGet("export-stockcard")]
        [Authorize]
        public async Task<IActionResult> ExportStockCardPdf(
            [FromQuery] int warehouseId,
            [FromQuery] int productId,
            [FromQuery] string from,
            [FromQuery] string to)
        {
            try
            {
                // Parse 'from' và 'to' thành Instant theo định dạng ISO-8601
                var parseFrom = InstantPattern.ExtendedIso.Parse(from);
                var parseTo = InstantPattern.ExtendedIso.Parse(to);
                if (!parseFrom.Success || !parseTo.Success)
                {
                    return BadRequest(new BaseResponse
                    {
                        Message = "Invalid date format. Please use ISO-8601 strings, e.g. 2025-01-01T00:00:00Z"
                    });
                }
                var startDate = parseFrom.Value;
                var endDate = parseTo.Value;
                var pdfBytes = await _reportService.ExportStockCardPdf(warehouseId, productId, startDate, endDate);
                return File(pdfBytes, "application/pdf", $"StockCard_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
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

