using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using Newtonsoft.Json;
using NodaTime;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
        {
            if (!context.Request.Headers.ContainsKey("Authorization") || context.Request.Path.ToString().Contains("login", StringComparison.OrdinalIgnoreCase) || context.Request.Path.ToString().Contains("refreshToken", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (context.Request.Method == HttpMethods.Post ||
                context.Request.Method == HttpMethods.Put ||
                context.Request.Method == HttpMethods.Delete)
            {
                var originalBodyStream = context.Response.Body;

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context);

                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    context.Response.Body.Seek(0, SeekOrigin.Begin);

                    await responseBody.CopyToAsync(originalBodyStream);

                    var accountId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (Guid.TryParse(accountId, out var parsedAccountId))
                    {
                        var auditLog = new AuditLogs
                        {
                            Date = SystemClock.Instance.GetCurrentInstant(),
                            Resource = context.Request.Path,
                            Action = context.Request.Method,
                            Payload = JsonConvert.SerializeObject(new
                            {
                                Request = await FormatRequest(context.Request),
                                Response = string.IsNullOrEmpty(Utils.Base64Encode(responseText)) ? $"Status Code: {context.Response.StatusCode}" : Utils.Base64Encode(responseText)
                            }),
                            AccountId = parsedAccountId
                        };

                        await unitOfWork.AuditLogsRepository.CreateAsync(auditLog);
                        await unitOfWork.SaveChangesAsync();
                    }
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);
            return text;
        }
    }
}
