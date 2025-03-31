using DrugWarehouseManagement.API.Middleware;
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
using Hangfire;
using Hangfire.PostgreSql;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using System.Text;
using System.Text.Json.Serialization;
using Minio;
using Minio.DataModel.Args;
using System;
using NodaTime;

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

            services.AddHangfire((provider, config) =>
            {
                config.UseSimpleAssemblyNameTypeSerializer();
                config.UseRecommendedSerializerSettings();
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);

                // Use PostgreSQL as storage
                config.UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"));
                });

                // Use our custom JobActivator
                config.UseActivator(new ScopedJobActivator(provider.GetRequiredService<IServiceScopeFactory>()));
            });
            services.AddHangfireServer();
            services.AddAuthorizeService(configuration);

            AddMapper();
            AddEnum(services);
            AddCors(services);

            var accessKey = configuration["Minio:AccessKey"];
            var secretKey = configuration["Minio:SecretKey"];
            var endpoint = configuration["Minio:Endpoint"];
            var ssl = configuration.GetValue<bool>("Minio:SSL");
            InitializeMinio(services, accessKey, secretKey, endpoint, ssl);

            InitializeFirebase();

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
            services.AddScoped<ILotTransferService, LotTransferService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IMinioService, MinioService>();
            services.AddScoped<IFirebaseService, FirebaseService>();
            services.AddScoped<ICategoriesService, CategoriesService>();
            services.AddScoped<IInventoryReportService, InventoryReportService>();
            services.AddScoped<IReturnOutboundService,ReturnOutboundService >();
            //services.AddScoped<IInventoryReportService, InventoryReportService>();

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero
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
            TypeAdapterConfig<OutboundDetails, OutboundDetailRespone>
            .NewConfig()
            .Map(dest => dest.ProductName, src => src.Lot.Product.ProductName)
            .Map(dest => dest.UnitType, src => src.Lot.Product.SKU)
            .Map(dest => dest.LotNumber, src => src.Lot.LotNumber);  
            TypeAdapterConfig<Outbound, OutboundResponse>
            .NewConfig()
            .Map(dest => dest.CustomerName, src => src.Customer.CustomerName)
            .Map(dest => dest.PhoneNumber, src => src.Customer.PhoneNumber)
            .Map(dest => dest.OutboundDetails, src => src.OutboundDetails);
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

            TypeAdapterConfig<LotTransfer, CreateLotTransferResponse>
                .NewConfig()
                .Map(dest => dest.LotTransferStatus, src => src.LotTransferStatus.ToString())
                .Map(dest => dest.FromWareHouse, src => src.FromWareHouse.WarehouseName)
                .Map(dest => dest.ToWareHouse, src => src.ToWareHouse.WarehouseName)
                .Map(dest => dest.Username, src => src.Account.UserName);

            TypeAdapterConfig<LotTransfer, ViewLotTransfer>
                .NewConfig()
                .Map(dest => dest.LotTransferStatus, src => src.LotTransferStatus.ToString())
                .Map(dest => dest.FromWareHouse, src => src.FromWareHouse.WarehouseName)
                .Map(dest => dest.ToWareHouse, src => src.ToWareHouse.WarehouseName)
                .Map(dest => dest.CreatedBy, src => src.Account.FullName)
                .IgnoreNullValues(true);

            TypeAdapterConfig<LotTransferDetail, ViewLotTransferDetail>
                .NewConfig()

                .Map(dest => dest.LotNumber, src => src.Lot.LotNumber)
                .IgnoreNullValues(true);

            TypeAdapterConfig<UpdateLotTransferRequest, LotTransfer>
                .NewConfig()
                .IgnoreNullValues(true);

            TypeAdapterConfig<Categories, ViewCategories>
                .NewConfig()
                .Map(dest => dest.ParentCategoryName, src => src.ParentCategory.CategoryName);

            TypeAdapterConfig<UpdateCategoryRequest, Categories>
                .NewConfig()
                .IgnoreNullValues(true)
                .Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId);

            // Set up time zone (example: UTC+7 for Vietnam)
            DateTimeZone timeZone = DateTimeZoneProviders.Tzdb["Asia/Ho_Chi_Minh"];

            TypeAdapterConfig<Inbound, ViewInbound>
                .NewConfig()
                .Map(dest => dest.ProviderName, src => src.Provider.ProviderName)
                .Map(dest => dest.CreateBy, src => src.Account.FullName)
                .Map(dest => dest.WarehouseName, src => src.Warehouse.WarehouseName);

            TypeAdapterConfig<InboundDetails, InboundDetailResponse>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product.ProductName);

            TypeAdapterConfig<Lot, ViewLot>
                .NewConfig()
                .Map(dest => dest.ProviderName, src => src.Provider.ProviderName)
                .Map(dest => dest.ProductName, src => src.Product.ProductName)
                .Map(dest => dest.WarehouseName, src => src.Warehouse.WarehouseName);

            TypeAdapterConfig<ReturnOutboundDetails, ReturnOutboundDetailsResponse>
                .NewConfig()
                .Map(dest => dest.OutboundCode, src => src.OutboundDetails.Outbound.OutboundCode)
                .Map(dest => dest.ProductCode, src => src.OutboundDetails.Lot.Product.ProductCode)
                .Map(dest => dest.ProductName, src => src.OutboundDetails.Lot.Product.ProductName);
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

        private static void InitializeFirebase()
        {
            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    FirebaseApp.Create(new AppOptions()
            //    {
            //        Credential = GoogleCredential.FromFile("firebase-credentials.json")
            //    });

            //}
        }

        private static void InitializeMinio(IServiceCollection services, string accessKey, string secretKey, string endpoint, bool ssl = false)
        {
            services.AddMinio(configureClient => configureClient
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(ssl)
                .Build());
        }

    }
}
