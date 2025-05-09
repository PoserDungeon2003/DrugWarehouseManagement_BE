﻿using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;

        public AccountController(IAccountService accountService, INotificationService notificationService)
        {
            _accountService = accountService;
            _notificationService = notificationService;
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
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _accountService.SetupTwoFactorAuthenticator(Guid.Parse(accountId));
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

        [HttpPost("confirmSetup2FA")]
        [Authorize]
        public async Task<IActionResult> ConfirmSetupTwoFactorAuthenticator([FromBody] ConfirmSetupTwoFactorAuthenticatorRequest request)
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var response = await _accountService.ConfirmSetupTwoFactorAuthenticator(Guid.Parse(accountId), request);
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

        [HttpPost("refreshToken")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _accountService.GenerateRefreshToken(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new BaseResponse
                {
                    Code = 401,
                    Message = ex.Message,
                });
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

        [HttpPost("resetPassword/{email}")]
        public async Task<IActionResult> ResetPassword([FromRoute][Required][EmailAddress] string email)
        {
            try
            {
                var response = await _accountService.ResetPassword(email);
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

        [HttpGet("notifications")]
        [Authorize]
        public async Task<IActionResult> GetNotificationsByRole([FromQuery] QueryPaging request)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            try
            {
                var response = await _notificationService.GetNotificationsByRole(request, role);
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

        [HttpPost("read-notifications")]
        [Authorize]
        public async Task<IActionResult> ReadAllNotifications()
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            try
            {
                var response = await _notificationService.ReadAllNotifications(role);
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
