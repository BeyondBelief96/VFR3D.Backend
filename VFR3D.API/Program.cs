using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Options;
using Npgsql;
using NSwag;
using NSwag.Generation.Processors.Security;
using VFR3D.API.Authentication;
using VFR3D.API.Middleware;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Repositories;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Determine if running in Docker
bool isRunningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true, reloadOnChange: true);

// Setup CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});


// Setup Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddAzureWebAppDiagnostics();

    // Add Application Insights if connection string is configured
    var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    if (!string.IsNullOrEmpty(appInsightsConnectionString))
    {
        builder.Services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = appInsightsConnectionString;
        });
    }
}

builder.Services.Configure<AzureFileLoggerOptions>(options =>
{
    options.FileName = "VFR3D-api-log";
    options.FileSizeLimit = 50 * 1024;
    options.RetainedFileCountLimit = 5;
});

// Setup Controller Json Serialization Handling
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Setup Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var auth0Settings = builder.Configuration.GetSection("Auth0Settings").Get<Auth0Settings>();
        Auth0Handler.ConfigureJwtBearer(options, auth0Settings);

        // Only disable HTTPS requirement in development
        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
        else
        {
            options.RequireHttpsMetadata = true; // Explicitly require HTTPS in production
        }
    })
    .AddScheme<AuthenticationSchemeOptions, ConditionalAuthHandler>("Conditional", null);


// Setup Swagger
builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "VFR3D API";
    options.Version = "v1";

    options.AddSecurity("JWT", [],
        new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.ApiKey,
            Name = "Authorization",
            In = OpenApiSecurityApiKeyLocation.Header,
            Description = "Enter your Bearer token in the format: Bearer {token}"
        });

    // Add security requirement to all operations
    options.OperationProcessors.Add(
        new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

// Setup Environment Variable Settings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0Settings"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<PreflightApiSettings>(builder.Configuration.GetSection("PreflightApi"));

// Setup DB Context
builder.Services.AddDbContext<VFR3DDbContext>((serviceProvider, options) =>
{
    var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    var connectionString = dbSettings.GetConnectionString();

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();

    options.UseNpgsql(dataSourceBuilder.Build(),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(3);
            npgsqlOptions.CommandTimeout(30);
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
}, ServiceLifetime.Scoped);

// Configure Services
builder.Services.AddMemoryCache();
builder.Services.AddCloudStorageServices(builder.Configuration);
builder.Services.AddScoped<IAircraftPerformanceProfileRepository, AircraftPerformanceProfileRepository>();
builder.Services.AddScoped<IAircraftPerformanceProfileService, AircraftPerformanceProfileService>();
builder.Services.AddScoped<IAircraftService, AircraftService>();
builder.Services.AddScoped<IWeightBalanceProfileService, WeightBalanceProfileService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<ConditionalAuthHandler>();

// Register PreflightApiClient with typed HttpClient
builder.Services.AddHttpClient<IPreflightApiClient, PreflightApiClient>();

var app = builder.Build();

// Initialize Azure Blob Storage resources on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var cloudStorageInitService = scope.ServiceProvider.GetRequiredService<ICloudStorageInitializationService>();

    logger.LogInformation("Initializing Azure Blob Storage resources during startup...");
    try
    {
        await cloudStorageInitService.InitializeAsync(CancellationToken.None);
        logger.LogInformation("Azure Blob Storage resources initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize Azure Blob Storage resources");
    }
}

app.UseCors("AllowedOrigins");
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
