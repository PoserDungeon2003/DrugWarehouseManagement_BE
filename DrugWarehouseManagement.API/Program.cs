using DrugWarehouseManagement.API.Middleware;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Hangfire;

namespace DrugWarehouseManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            // Test Video Detection
            builder.Services.AddSingleton<VideoDetectionService>(provider =>
            {
                string modelPath = @"D:\DoAn-FPT\real\DrugWarehouseManagement_BE\yolov8n.onnx";
                return new VideoDetectionService(modelPath);
            });

            ServiceRegister.RegisterServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            if (!app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<AuditLoggingMiddleware>();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseCors("AllowAll");
            app.UseHangfireDashboard("/hangfire");
            app.UseAuthentication();
            app.UseMiddleware<ConcurrencyMiddleware>();
            app.UseAuthorization();

            app.MapControllers();
            ConfigureHangfireJobs(app);
            app.Run();
        }
        private static void ConfigureHangfireJobs(WebApplication app)
        {
            RecurringJob.AddOrUpdate<IInventoryService>(
                "CheckLowStockAndExpiry",
                service => service.NotifyLowStockAndExpiryAsync(),
                Cron.Daily
            );
        }
    }
}
