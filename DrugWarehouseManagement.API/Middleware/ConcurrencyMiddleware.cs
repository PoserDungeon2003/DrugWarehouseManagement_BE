using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Middleware
{
    public class ConcurrencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ConcurrencyMiddleware> _logger;

        public ConcurrencyMiddleware(RequestDelegate next, ILogger<ConcurrencyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, IUnitOfWork unitOfWork)
        {
            if (!context.Request.Headers.ContainsKey("Authorization") ||
                context.Request.Path.ToString().Contains("login", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.ToString().Contains("refreshToken", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenConcurrencyStamp = context.User.FindFirstValue("ConcurrencyStamp");

            if (userId == null || tokenConcurrencyStamp == null)
            {
                await _next(context);
                return;
            }

            var user = await unitOfWork.AccountRepository.GetByIdAsync(Guid.Parse(userId));

            if (user == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                var response = new BaseResponse
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Không tìm thấy tài khoản.",
                };

                var responseText = System.Text.Json.JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(responseText);
                return;
            }

            if (user.ConcurrencyStamp != tokenConcurrencyStamp)
            {
                _logger.LogInformation($"Concurrency conflict detected for user {userId}.");
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                var response = new BaseResponse
                {
                    Code = StatusCodes.Status409Conflict,
                    Message = "Có người khác đã đăng nhập tài khoản này. Vui lòng đăng nhập lại.",
                };

                var responseText = System.Text.Json.JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(responseText);
                return;
            }

            await _next(context);
        }

    }
}
