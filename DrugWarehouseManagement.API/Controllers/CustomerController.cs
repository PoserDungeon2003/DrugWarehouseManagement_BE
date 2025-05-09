﻿using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var response = await _customerService.CreateCustomerAsync(request);
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

        [HttpGet("{customerId}")]
        [Authorize]
        public async Task<IActionResult> GetCustomerDetailById(int customerId)
        {
            try
            {
                var response = await _customerService.GetCustomerDetailByIdAsync(customerId);
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

        [HttpPut("{customerId}")]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> UpdateCustomer(int customerId, [FromBody] UpdateCustomerRequest request)
        {
            try
            {
                await _customerService.UpdateCustomerAsync(customerId, request);
                return Ok(new BaseResponse
                {
                    Message = "Cập nhật khách hàng thành công",
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

        [HttpDelete("{customerId}")]
        [Authorize(Roles = "Sale Admin")]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            try
            {
                var response = await _customerService.DeleteCustomerAsync(customerId);
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

        [HttpGet("loyal")]
        [Authorize]
        public async Task<IActionResult> GetLoyalCustomers()
        {
            try
            {
                var response = await _customerService.GetLoyalCustomersAsync();
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
        [Authorize]
        public async Task<IActionResult> SearchCustomers([FromQuery] SearchCustomerRequest request)
        {
            try
            {
                var response = await _customerService.SearchCustomersAsync(request);
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
