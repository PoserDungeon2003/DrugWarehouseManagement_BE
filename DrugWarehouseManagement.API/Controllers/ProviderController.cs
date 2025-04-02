using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderService _providerService;   
        public ProviderController(IProviderService providerService)
        {
            _providerService = providerService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateProdivder([FromBody] CreateProviderRequest request)
        {
            try
            {
                var response = await _providerService.CreateProviderAsync(request);
                return Ok(response);
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
        [HttpDelete("{providerId}")]
        public async Task<IActionResult> DeleteProvider(int providerId)
        {
            try
            {
                var response = await _providerService.DeleteProviderAsync(providerId);
                return Ok(response);
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
        [HttpPut("{providerId}")]
        public async Task<IActionResult> UpdateProvider(int providerId, [FromBody] UpdateProviderRequest request)
        {
            try
            {
                var response = await _providerService.UpdateProviderAsync(providerId, request);
                return Ok(response);
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
        [HttpGet]
        public async Task<IActionResult> SearchProviders([FromQuery] QueryPaging queryPaging)
        {
            try
            {
                var result = await _providerService.SearchProvidersAsync(queryPaging);
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

        [HttpGet("{providerId}")]
        public async Task<IActionResult> GetProvider(int providerId)
        {
            try
            {
                var result = await _providerService.GetProviderByIdAsync(providerId);
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
