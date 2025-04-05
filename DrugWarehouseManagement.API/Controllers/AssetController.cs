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

        [HttpGet("inbound/{inboundId}/{id}")]
        [Authorize]
        public async Task<IActionResult> GetInboundAsset(string inboundId, string id)
        {
            try
            {
                var response = await _minioService.GetFileAsync($"drugwarehouse", $"{inboundId}/{id}");
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

        [HttpGet("inbound-request/{filename}")]
        [Authorize]
        public async Task<IActionResult> GetInboundRequestAsset(string filename)
        {
            try
            {
                var response = await _minioService.GetFileAsync($"inboundrequest", $"/{filename}");
                return File(response.ToArray(), "application/octet-stream", filename);
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
        [HttpGet("inbound-report/{filename}")]
        [Authorize]
        public async Task<IActionResult> GetInboundReportAsset(string filename)
        {
            try
            {
                var response = await _minioService.GetFileAsync($"inboundreport", $"/{filename}");
                return File(response.ToArray(), "application/octet-stream", filename);
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
