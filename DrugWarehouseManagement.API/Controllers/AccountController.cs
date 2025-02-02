using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DrugWarehouseManagement.Service.DTO.Request;
using System.Net;
using Azure.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginWithUsername([FromBody] AccountLoginRequest request)
        {
            try
            {
                var response = await _accountService.LoginWithUsername(request);
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

        [HttpPost("setupTwoFactorAuthenticator")]
        [Authorize]
        public async Task<IActionResult> SetupTwoFactorAuthenticator()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            try
            {
                var response = await _accountService.SetupTwoFactorAuthenticator(email);
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

        [HttpPatch("updateAccountSettings")]
        [Authorize]
        public async Task<IActionResult> UpdateAccountSettings([FromBody] UpdateAccountSettingsRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _accountService.UpdateAccountSettings(Guid.Parse(accountId), request);
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

        [HttpGet("/whoami")]
        [Authorize]
        public async Task<IActionResult> WhoAmI()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _accountService.GetAccountById(Guid.Parse(accountId));
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

        [HttpPut("updateAccount")]
        [Authorize]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _accountService.UpdateAccount(Guid.Parse(accountId), request);
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

        [HttpPut("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _accountService.ChangePassword(Guid.Parse(accountId), request);
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
