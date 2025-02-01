using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
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

        [HttpGet("activeAccount")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActiveAccount([FromQuery] [Required] Guid accountId)
        {
            try
            {
                var response = await _accountService.ActiveAccount(accountId);
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

        [HttpGet("deactiveAccount")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactiveAccount([FromQuery] [Required] Guid accountId)
        {
            try
            {
                var response = await _accountService.DeactiveAccount(accountId);
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

        [HttpDelete("deleteAccount")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAccount([FromQuery] [Required] Guid accountId)
        {
            try
            {
                var response = await _accountService.DeleteAccount(accountId);
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

        [HttpPost("resetPassword")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetPassword([FromQuery] [Required] Guid accountId)
        {
            try
            {
                var response = await _accountService.ResetPassword(accountId);
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
