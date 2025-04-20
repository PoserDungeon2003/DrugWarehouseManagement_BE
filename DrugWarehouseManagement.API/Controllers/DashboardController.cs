using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [HttpGet("report")]
        public async Task<IActionResult> GetDashboardReport([FromQuery] TimeFilterOption filter)
        {
            try
            {
                var roles = User.FindFirstValue(ClaimTypes.Role);
                var dashboard = await _dashboardService.GetDashboardReportAsync(roles, filter);
                return Ok(dashboard);
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

