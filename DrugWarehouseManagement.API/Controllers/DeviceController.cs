using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost("registerDevice")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
        {
            try
            {
                var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _deviceService.RegisterDevice(Guid.Parse(accountId), request);
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

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                var apiKey = GetApiKey();
                if (apiKey == null)
                {
                    return Unauthorized(new BaseResponse
                    {
                        Code = 401,
                        Message = "API Key is missing."
                    });
                }
                var response = await _deviceService.Ping(apiKey);
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

        [HttpPatch("updateTrackingNumber")]
        public async Task<IActionResult> UpdateTrackingNumber([FromBody] UpdateTrackingNumberRequest request)
        {
            try
            {
                var apiKey = GetApiKey();
                if (apiKey == null)
                {
                    return Unauthorized(new BaseResponse
                    {
                        Code = 401,
                        Message = "API Key is missing."
                    });
                }
                var response = await _deviceService.UpdateTrackingNumber(apiKey, request);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDevices([FromQuery] QueryPaging queryPaging)
        {
            try
            {
                var response = await _deviceService.GetDevices(queryPaging);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDevice([FromBody] UpdateDeviceRequest request)
        {
            try
            {
                var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _deviceService.UpdateDevice(Guid.Parse(accountId), request);
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

        [HttpDelete("{deviceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDevice(int deviceId)
        {
            try
            {
                var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _deviceService.DeleteDevice(Guid.Parse(accountId), deviceId);
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

        private string GetApiKey()
        {
            if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
            {
                return null;
            }
            return apiKey;
        }
    }
}
