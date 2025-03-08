using DrugWarehouseManagement.Service.DTO.Response;

namespace DrugWarehouseManagement.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                if (!context.Response.HasStarted &&
                    (context.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                     context.Response.StatusCode == StatusCodes.Status403Forbidden))
                {
                    await HandleUnauthorizedOrForbiddenResponse(context);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var response = new BaseResponse
                {
                    Code = 500,
                    Message = "An internal server error occurred.",
                    Details = !context.RequestServices.GetRequiredService<IHostEnvironment>().IsProduction()
                        ? ex.StackTrace
                        : null
                };

                var responseText = System.Text.Json.JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(responseText);
            }
        }

        private async Task HandleUnauthorizedOrForbiddenResponse(HttpContext context)
        {
            var statusCode = context.Response.StatusCode;
            var message = statusCode == StatusCodes.Status401Unauthorized ? "Unauthorized access" : "Forbidden access";
            context.Response.ContentType = "application/json";

            _logger.LogWarning($"Unauthorized/Forbidden request. Status code: {statusCode}");

            var response = new BaseResponse
            {
                Code = statusCode,
                Message = message,
            };

            var responseText = System.Text.Json.JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(responseText);
        }
    }
}
