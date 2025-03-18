using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
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
        public IActionResult ExportInventoryReport(
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
                var fileBytes = _reportService.ExportInventoryReport(warehouseId, startDate, endDate);
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "InventoryReport.xlsx");
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

