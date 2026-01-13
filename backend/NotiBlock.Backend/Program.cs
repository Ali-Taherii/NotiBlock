using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotiBlock.Backend.Data;
using NotiBlock.Backend.Interfaces;
using NotiBlock.Backend.Middleware;
using NotiBlock.Backend.Services;
using Serilog;
using System.Text;

// ===== INITIAL SETUP AND ENVIRONMENT LOADING =====

// Load environment variables from .env file
Env.Load();

// Ensure logs directory exists
var logsDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsDir))
{
    Directory.CreateDirectory(logsDir);
}

// Configure Serilog before building the app
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logsDir, "notiblock-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("========== STARTING NOTIBLOCK API ==========");
    Log.Information("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_Environment"));
    Log.Information("Current Directory: {CurrentDirectory}", Directory.GetCurrentDirectory());

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // ===== CONFIGURATION =====

    Log.Information("Loading configuration...");
    builder.Configuration
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>(optional: true);

    Log.Information("Configuration loaded successfully");

    // ===== SERVICES REGISTRATION =====

    Log.Information("Registering services...");

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        })
        .ConfigureApiBehaviorOptions(options =>
        {
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

    Log.Information("Configuring database...");
    var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(defaultConnection))
    {
        Log.Warning("DefaultConnection not configured - Database will be skipped");
    }
    else
    {
        Log.Information("Registering DbContext with connection string");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(defaultConnection));
    }

    // ===== DEPENDENCY INJECTION =====

    Log.Information("Registering application services...");
    builder.Services.AddScoped<IRecallService, RecallService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IConsumerReportService, ConsumerReportService>();
    builder.Services.AddScoped<IResellerTicketService, ResellerTicketService>();
    builder.Services.AddScoped<IRegulatorReviewService, RegulatorReviewService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

    // ===== CORS CONFIGURATION =====

    Log.Information("Configuring CORS...");
    var corsOrigin = builder.Configuration.GetValue<string>("CorsSettings:AllowedOrigin")
        ?? "http://localhost:5173";

    Log.Information("CORS Origin: {CorsOrigin}", corsOrigin);

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

    // ===== JWT AUTHENTICATION CONFIGURATION =====

    Log.Information("Configuring JWT authentication...");
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var jwtKey = jwtSettings["Key"];
    var jwtIssuer = jwtSettings["Issuer"];
    var jwtAudience = jwtSettings["Audience"];

    var jwtConfigured = false;

    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        Log.Warning("JWT Key is not configured");
    }
    else if (string.IsNullOrWhiteSpace(jwtIssuer))
    {
        Log.Warning("JWT Issuer is not configured");
    }
    else if (string.IsNullOrWhiteSpace(jwtAudience))
    {
        Log.Warning("JWT Audience is not configured");
    }
    else
    {
        try
        {
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
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

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

            jwtConfigured = true;
            Log.Information("JWT authentication configured successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure JWT authentication");
        }
    }

    // ===== BUILD APPLICATION =====

    Log.Information("Building application...");
    var app = builder.Build();

    // ===== MIDDLEWARE CONFIGURATION =====

    Log.Information("Configuring middleware...");

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        Log.Information("Development environment detected - Enabling Swagger");
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowFrontend");
    app.UseHttpsRedirection();

    if (jwtConfigured)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    app.MapControllers();

    Log.Information("========== APPLICATION CONFIGURED SUCCESSFULLY ==========");
    Log.Information("Server addresses: {ServerAddresses}", string.Join(", ", app.Urls));
    Log.Information("Starting to listen on configured URLs...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("Shutting down NotiBlock API");
    Log.CloseAndFlush();
}