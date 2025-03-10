using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssetController : ControllerBase
    {
        private readonly IMinioService _minioService;
        public AssetController(IMinioService minioService)
        {
            _minioService = minioService;
        }

        [HttpGet("inbound/{inboundId}/report/{id}")]
        [Authorize]
        public async Task<IActionResult> GetInboundReportAsset(string inboundId, string id)
        {
            try
            {
                var response = await _minioService.GetFileAsync($"drugwarehouse-reports/{inboundId}", id);
                return File(response.ToArray(), "application/octet-stream", id);
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
