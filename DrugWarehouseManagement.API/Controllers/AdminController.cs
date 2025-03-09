using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AdminController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("createAccount")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            try
            {
                var response = await _accountService.CreateAccount(request);
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

        [HttpGet("getAccounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAccountsPaginatedAsync([FromQuery] QueryPaging request)
        {
            try
            {
                var response = await _accountService.GetAccountsPaginatedAsync(request);
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

        [HttpGet("activeAccount/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActiveAccount([FromRoute][Required] Guid id)
        {
            try
            {
                var response = await _accountService.ActiveAccount(id);
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

        [HttpGet("deactiveAccount/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactiveAccount([FromRoute][Required] Guid id)
        {
            try
            {
                var response = await _accountService.DeactiveAccount(id);
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

        [HttpDelete("deleteAccount/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount([FromRoute][Required] Guid id)
        {
            try
            {
                var response = await _accountService.DeleteAccount(id);
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

        [HttpPost("resetPassword/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword([FromRoute][Required] Guid id)
        {
            try
            {
                var response = await _accountService.ResetPassword(id);
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

        [HttpPost("reset2FA/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminReset2FA([FromRoute][Required] Guid id)
        {
            try
            {
                var response = await _accountService.AdminReset2FA(id);
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
