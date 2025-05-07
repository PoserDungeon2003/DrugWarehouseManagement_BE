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
using StackExchange.Redis;
using DrugWarehouseManagement.Service.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DrugWarehouseManagement.API
{
    public static class ServiceRegister
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddSingleton<NpgsqlDataSource>(sp =>
            {
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("DefaultConnection"));
                dataSourceBuilder.UseNodaTime(); // Retain NodaTime configuration
                dataSourceBuilder.EnableDynamicJson(); // Enable dynamic JSON serialization
                dataSourceBuilder.UseJsonNet(); // Use Newtonsoft.Json if preferred
                return dataSourceBuilder.Build();
            });

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfiguration = configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(redisConfiguration);
            });


            services.AddDbContext<DrugWarehouseContext>((sp, options) =>
            {
                var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
                options.UseNpgsql(dataSource, o =>
                {
                    o.UseNodaTime();
                });
            });

            // Lấy modelPath từ appsettings.json
            string modelPath = configuration.GetValue<string>("YoloModel:ModelPath");

            if (string.IsNullOrEmpty(modelPath))
            {
                throw new InvalidOperationException("YOLO model path is not configured in appsettings.json.");
            }

            services.AddSingleton<VideoDetectionService>(provider => new VideoDetectionService(modelPath));

            if (env.IsProduction())
            {
                services.AddHangfire((prov, cfg) =>
                {
                    cfg.UseSimpleAssemblyNameTypeSerializer()
                       .UseRecommendedSerializerSettings()
                       .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                       .UsePostgreSqlStorage(opt => opt.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection")))
                       .UseActivator(new ScopedJobActivator(prov.GetRequiredService<IServiceScopeFactory>()));
                });
                services.AddHangfireServer();
            }
            services.AddAuthorizeService(configuration);

            AddMapper();
            AddEnum(services);
            AddCors(services);

            var accessKey = configuration["Minio:AccessKey"];
            var secretKey = configuration["Minio:SecretKey"];
            var endpoint = configuration["Minio:Endpoint"];
            var ssl = configuration.GetValue<bool>("Minio:SSL");
            InitializeMinio(services, accessKey, secretKey, endpoint, ssl);

            // InitializeFirebase();

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
            services.AddScoped<IInboundRequestService, InboundRequestService>();
            services.AddScoped<IInboundReportService, InboundReportService>();
            services.AddScoped<IInventoryCheckService, InventoryCheckService>();
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
            services.AddScoped<IDashboardService, DashboardService>();
            //services.AddScoped<IInventoryReportService, InventoryReportService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IInboundRequestService, InboundRequestService>();
            services.AddScoped<INotificationService, NotificationService>();
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

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine($"Authentication challenge: {context.ErrorDescription}");
                        return Task.CompletedTask;
                    }
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
            .Map(dest => dest.LotNumber, src => src.Lot.LotNumber)
              .Map(d => d.WarehouseId, src => src.Lot.Warehouse.WarehouseId)
            .Map(d => d.WarehouseCode, src => src.Lot.Warehouse.WarehouseCode)
            .Map(d => d.WarehouseName, src => src.Lot.Warehouse.WarehouseName);
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
            TypeAdapterConfig<UpdateProductRequest, Product>
                .NewConfig()
                .IgnoreNullValues(true)
                .Ignore(dest => dest.ProductCategories);
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
                .Map(dest => dest.ExpiryDate, src => src.Lot.ExpiryDate)
                .Map(dest => dest.LotNumber, src => src.Lot.LotNumber)
                .Map(dest => dest.ProductName, src => src.Lot.Product.ProductName)
                .Map(dest => dest.Quantity, src => src.Lot.Quantity)
                .IgnoreNullValues(true);
            TypeAdapterConfig<UpdateCustomerRequest, Customer>
            .NewConfig()
            .IgnoreNullValues(true);
            TypeAdapterConfig<UpdateProviderRequest, Provider>
                .NewConfig()
    .           IgnoreNullValues(true);
            TypeAdapterConfig<UpdateLotTransferRequest, LotTransfer>
                .NewConfig()
                .IgnoreNullValues(true);
            TypeAdapterConfig<UpdateOutboundRequest, Outbound>
                .NewConfig()
                 .Map(dest => dest.ReceiverName, src => src.CustomerName)
                 .Map(dest => dest.ReceiverPhone, src => src.PhoneNumber)
                 .Map(dest => dest.ReceiverAddress, src => src.Address)
                 .Map(dest => dest.OutboundOrderCode, src => src.OutboundOrderCode)
                 .Map(dest => dest.Note, src => src.Note)
                 .IgnoreNullValues(true)
                 .Ignore(dest => dest.Status);
            TypeAdapterConfig<Categories, ViewCategories>
                .NewConfig()
                .Map(dest => dest.ParentCategoryName, src => src.ParentCategory.CategoryName);

            TypeAdapterConfig<UpdateCategoryRequest, Categories>
                .NewConfig()
                .IgnoreNullValues(true)
                .Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId);

            TypeAdapterConfig<Inbound, ViewInbound>
                .NewConfig()
                .Map(dest => dest.CreateBy, src => src.Account.FullName)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.WarehouseName, src => src.Warehouse.WarehouseName)
                .Map(dest => dest.ProviderDetails, src => src.Provider);
            // Configure InboundReport to ViewInboundReport
            TypeAdapterConfig<InboundReport, ViewInboundReport>.NewConfig()
                .Map(dest => dest.InboundReportId, src => src.InboundReportId)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.ProblemDescription, src => src.ProblemDescription);

            TypeAdapterConfig<InboundDetails, InboundDetailResponse>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product.ProductName);

            TypeAdapterConfig<InboundRequest, ViewInboundRequest>
                .NewConfig()
                .Map(dest => dest.CreateDate, src => src.CreatedAt)
                .Map(dest => dest.Status, src => src.Status.ToString());

            TypeAdapterConfig<InboundRequestDetails, InboundRequestDetailResponse>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product.ProductName);

            TypeAdapterConfig<InventoryCheck, ViewInventoryCheck>
                .NewConfig()
                .Map(dest => dest.Warehouse, src => src.Warehouse)
                .Map(dest => dest.Details, src => src.InventoryCheckDetails);

            TypeAdapterConfig<InventoryCheckDetail, InventoryCheckDetailReponse>
                .NewConfig()
                .Map(dest => dest.LotNumber, src => src.Lot.LotNumber)
                .Map(dest => dest.ProductName, src => src.Lot.Product.ProductName)
                .Map(dest => dest.SKU, src => src.Lot.Product.SKU);

            TypeAdapterConfig<Asset, AssetResponse>
                .NewConfig()
                .Map(dest => dest.FileUrl, src => src.FileUrl)
                .Map(dest => dest.FileName, src => src.FileName);

            TypeAdapterConfig<Lot, ViewLot>
                .NewConfig()
                .Map(dest => dest.ProviderName, src => src.Provider.ProviderName)
                .Map(dest => dest.ProductName, src => src.Product.ProductName)
                .Map(dest => dest.ProductSKU, src => src.Product.SKU)
                .Map(dest => dest.WarehouseName, src => src.Warehouse.WarehouseName);

            TypeAdapterConfig<ReturnOutboundDetails, ReturnOutboundDetailsResponse>
                .NewConfig()
                .Map(dest => dest.OutboundCode, src => src.OutboundDetails.Outbound.OutboundCode)
                .Map(dest => dest.ProductCode, src => src.OutboundDetails.Lot.Product.ProductCode)
                .Map(dest => dest.ProductName, src => src.OutboundDetails.Lot.Product.ProductName);

            TypeAdapterConfig<Device, ViewDevices>
                .NewConfig()
                .Map(dest => dest.ExpiryDate, src => src.ExpiryDate.HasValue ? src.ExpiryDate.Value.ToString("o") : null)
                .Map(dest => dest.CreatedBy, src => src.Account.FullName)
                .Map(dest => dest.Status, src => src.Status.ToString());

            TypeAdapterConfig<UpdateDeviceRequest, Device>
                .NewConfig()
                .IgnoreNullValues(true);
            TypeAdapterConfig<Provider,ProviderResponse>
                .NewConfig()
                .Map(dest => dest.DocumentIssueDate, src => src.CreatedAt.ToString())
                .IgnoreNullValues(true);
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
                options.AddPolicy("Limited", builder =>
                {
                    builder.WithOrigins("https://trung-hanh-management-fe.vercel.app")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
                options.AddPolicy("AllowLocalHost", builder =>
                {
                    builder.WithOrigins("https://localhost:7031")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });
        }

        private static void InitializeFirebase()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
               FirebaseApp.Create(new AppOptions()
               {
                   Credential = GoogleCredential.FromFile("firebase-credentials.json")
               });

            }
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
