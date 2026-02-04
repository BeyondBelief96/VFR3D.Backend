using System.Text.Json.Serialization;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Extensions.Options;
using Npgsql;
using NSwag;
using NSwag.Generation.Processors.Security;
using VFR3D.API.Authentication;
using VFR3D.API.Configuration;
using VFR3D.API.Middleware;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Repositories;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.AirportInformationServices;
using VFR3D.Infrastructure.Services.DocumentServices;
using VFR3D.Infrastructure.Services.NotamServices;
using VFR3D.Infrastructure.Services.WeatherServices;
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

// Add Azure Key Vault for secrets
var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    var credential = new DefaultAzureCredential();

    // Map Key Vault secret names to configuration keys based on environment
    var secretSuffix = builder.Environment.IsProduction() ? "prd" : "staging";
    var secretMappings = new Dictionary<string, string>
    {
        { $"vfr3d-faa-nms-api-client-id-{secretSuffix}", "NmsSettings:ClientId" },
        { $"vfr3d-faa-nms-api-client-secret-{secretSuffix}", "NmsSettings:ClientSecret" }
    };

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        credential,
        new MappedKeyVaultSecretManager(secretMappings));
}

// Setup CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "https://www.vfr3d.com",
                    "https://vfr3d-frontend-git-develop-beyondbelief96s-projects.vercel.app/aircraft")
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
    options.JsonSerializerOptions.Converters.Add(new GeometryJsonConverter());
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
builder.Services.Configure<NOAASettings>(builder.Configuration.GetSection("NOAASettings"));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.Configure<Auth0Settings>(builder.Configuration.GetSection("Auth0Settings"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<NmsSettings>(builder.Configuration.GetSection("NmsSettings"));

// Setup DB Context
builder.Services.AddDbContext<VFR3DDbContext>((serviceProvider, options) =>
{
    var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
    var connectionString = dbSettings.GetConnectionString();

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.UseNetTopologySuite();
    dataSourceBuilder.EnableDynamicJson();

    options.UseNpgsql(dataSourceBuilder.Build(),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(3);
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.UseNetTopologySuite();
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
builder.Services.AddScoped<IMetarService, MetarService>();
builder.Services.AddScoped<IPirepService, PirepService>();
builder.Services.AddScoped<ITafService, TafService>();
builder.Services.AddScoped<IAirsigmetService, AirsigmetService>();
builder.Services.AddScoped<IGAirmetService, GAirmetService>();
builder.Services.AddScoped<IAirportDiagramService, AirportDiagramService>();
builder.Services.AddScoped<IChartSupplementService, ChartSupplementService>();  
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddScoped<IRunwayService, RunwayService>();
builder.Services.AddScoped<ICommunicationFrequencyService, CommunicationFrequencyService>();
builder.Services.AddScoped<IAirspaceService, AirspaceService>();
builder.Services.AddScoped<IObstacleService, ObstacleService>();
builder.Services.AddScoped<IMagneticVariationService, MagneticVariationService>();
builder.Services.AddScoped<IWindsAloftService, WindsAloftService>();
builder.Services.AddScoped<INavlogService, NavlogService>();
builder.Services.AddScoped<IAircraftPerformanceProfileService, AircraftPerformanceProfileService>();
builder.Services.AddScoped<IAircraftService, AircraftService>();
builder.Services.AddScoped<IWeightBalanceProfileService, WeightBalanceProfileService>();
builder.Services.AddScoped<IPerformanceCalculatorService, PerformanceCalculatorService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<ConditionalAuthHandler>();

// NOTAM Services
builder.Services.AddSingleton<INmsApiClient, NmsApiClient>();
builder.Services.AddScoped<INotamService, NotamService>();

builder.Services.AddHttpClient();

// Configure NMS API HttpClient with extended timeout
builder.Services.AddHttpClient("NmsApi", (serviceProvider, client) =>
{
    var nmsSettings = serviceProvider.GetRequiredService<IOptions<NmsSettings>>().Value;
    client.Timeout = TimeSpan.FromSeconds(nmsSettings.RequestTimeoutSeconds);
});

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
