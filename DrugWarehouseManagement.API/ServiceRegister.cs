using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository.Repositories;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Service.Wrapper;
using DrugWarehouseManagement.Service.Wrapper.Interface;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;
using System.Text.Json.Serialization;

namespace DrugWarehouseManagement.API
{
    public static class ServiceRegister
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<NpgsqlDataSource>(sp =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"));
                dataSourceBuilder.UseNodaTime(); // Retain NodaTime configuration
                dataSourceBuilder.EnableDynamicJson(); // Enable dynamic JSON serialization
                dataSourceBuilder.UseJsonNet(); // Use Newtonsoft.Json if preferred
                return dataSourceBuilder.Build();
            });

            services.AddDbContext<DrugWarehouseContext>((sp, options) =>
            {
                var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
                options.UseNpgsql(dataSource, o =>
                {
                    o.UseNodaTime();
                });
            });

            services.AddAuthorizeService(configuration);

            AddMapper();
            AddEnum(services);
            AddCors(services);

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenHandlerService, TokenHandlerService>();
            services.AddScoped<IPasswordWrapper, PasswordWrapper>();
            services.AddScoped<ITwoFactorAuthenticatorWrapper, TwoFactorAuthenticatorWrapper>();
            services.AddScoped<IPasswordHasher<Account>, PasswordHasher<Account>>();
            services.AddScoped<IPasswordWrapper, PasswordWrapper>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IInboundService, InboundService>();
            services.AddScoped<ILotService, LotService>();
            services.AddScoped<IOutboundService, OutboundService>();
            services.AddScoped<IAuditLogsRepository, AuditLogsRepository>();
            services.AddScoped<IAuditLogsService, AuditLogsService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<ICustomerService, CustomerService>();

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
            TypeAdapterConfig<Account, ViewAccount>
                .NewConfig()
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.RoleName, src => src.Role.RoleName);
            TypeAdapterConfig<UpdateWarehouseRequest, Warehouse>
               .NewConfig()
               .IgnoreNullValues(true);
            TypeAdapterConfig<UpdateAccountRequest, Account>
                .NewConfig()
                .IgnoreNullValues(true);
            TypeAdapterConfig<UpdateOutboundRequest, Outbound>
                .NewConfig()
                .IgnoreNullValues(true);
            TypeAdapterConfig<UpdateProductRequest, Product>
                .NewConfig()
                .IgnoreNullValues(true);
            TypeAdapterConfig<AuditLogs, ViewAuditLogs>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.Account.UserName)
                .Map(dest => dest.FullName, src => src.Account.FullName);
            //.Map(dest => dest.Date, src => src.Date.ToDateTimeUtc());
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
