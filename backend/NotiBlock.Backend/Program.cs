using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotiBlock.Backend.Configuration;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Middleware;
using NotiBlock.Backend.Services;
using Serilog;
using System.Text;

// ===== LOAD .ENV FILE FIRST =====
Env.Load();

// Configure Serilog before building the app
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/notiblock-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting NotiBlock API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // ===== LOAD CONFIGURATION FROM .ENV =====
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>(optional: true);

    // Add services to the container.
    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    
        // Make property names camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Customize automatic 400 responses for model validation errors
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err => err.ErrorMessage))
                .ToList();

            var response = new
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors
            };

            return new BadRequestObjectResult(response);
        };
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add Exception Handler
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ===== DATABASE CONFIGURATION =====
    var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    Log.Information("Database Connection String: {ConnectionString}", 
        defaultConnection == null ? "NOT CONFIGURED" : "Configured");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(defaultConnection));

    // ===== SERVICES REGISTRATION =====
    builder.Services.AddScoped<IRecallService, RecallService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IConsumerReportService, ConsumerReportService>();
    builder.Services.AddScoped<IResellerTicketService, ResellerTicketService>();
    builder.Services.AddScoped<IRegulatorReviewService, RegulatorReviewService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // ===== BLOCKCHAIN CONFIGURATION =====
    var blockchainSettings = builder.Configuration.GetSection("Blockchain");
    var rpcUrl = blockchainSettings["RpcUrl"];
    var privateKey = blockchainSettings["PrivateKey"];
    var contractAddress = blockchainSettings["ContractAddress"];

    Log.Information("Blockchain Configuration:");
    Log.Information("  RPC URL: {RpcUrl}", string.IsNullOrEmpty(rpcUrl) ? "NOT SET" : "Configured");
    Log.Information("  Private Key: {PrivateKey}", string.IsNullOrEmpty(privateKey) ? "NOT SET" : "Configured");
    Log.Information("  Contract Address: {ContractAddress}", string.IsNullOrEmpty(contractAddress) ? "NOT SET" : contractAddress);

    builder.Services.Configure<BlockchainSettings>(blockchainSettings);
    builder.Services.AddScoped<IBlockchainService, BlockchainService>();

    // Add CORS policy
    var corsOrigin = builder.Configuration.GetValue<string>("CorsSettings:AllowedOrigin") ?? "http://localhost:5173";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(corsOrigin)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });

    // JWT Token - Configure BEFORE building the app
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var jwtKey = jwtSettings["Key"];
    if (string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException("JWT Key is missing in configuration.");
    }
    var key = Encoding.ASCII.GetBytes(jwtKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        // Extract token from cookie instead of Authorization header
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["jwt_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();

    // Add Serilog request logging
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Use CORS before other middleware that uses endpoints
    app.UseCors("AllowFrontend");

    app.UseHttpsRedirection();

    // Use Authentication before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("NotiBlock API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

