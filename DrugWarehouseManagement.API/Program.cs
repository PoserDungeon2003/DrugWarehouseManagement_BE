using DrugWarehouseManagement.API.Middleware;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.Interface;
using Google;
using DrugWarehouseManagement.Service.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using DrugWarehouseManagement.Service.Hubs;

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
            builder.Services.AddSignalR();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
            });
            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
            });

            ServiceRegister.RegisterServices(builder.Services, builder.Configuration, builder.Environment);

            var app = builder.Build();
            
            app.UseRouting();
            if (!app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors("AllowAll");
            }
            else
            {
                app.UseCors("Limited");
                app.UseHangfireDashboard("/hangfire");
                RecurringJob.AddOrUpdate<IInventoryService>(
                    "CheckLowStockAndExpiry",
                    svc => svc.NotifyLowStockAndExpiryAsync(),
                    Cron.Daily
                );

            }

            app.UseMiddleware<AuditLoggingMiddleware>();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseAuthentication();
            app.UseMiddleware<ConcurrencyMiddleware>();
            app.UseAuthorization();
            app.MapHub<NotificationHub>("/notificationHub");

            app.MapControllers();
         
            app.Run();
        }
        
    }
}
