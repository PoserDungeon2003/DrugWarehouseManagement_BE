using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository.Repositories;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace DrugWarehouseManagement.API
{
    public static class ServiceRegister
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DrugWarehouseContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("local"), o =>
                {
                    o.UseNodaTime();
                });
            });

            services.AddAuthorizeService(configuration);

            AddMapper();
            AddEnum(services);
            AddCors(services);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<TokenHandlerService>();

            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();

        }

        public static IServiceCollection AddAuthorizeService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        private static void AddMapper()
        {

        }

        private static void AddEnum(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
                                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        }

        public static void AddCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }

    }
}
