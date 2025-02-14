using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSwag;
using NSwag.Generation.Processors.Security;
using VFR3D.API.Authentication;
using VFR3D.Cron.API.Extensions;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.AirportInformationServices;
using VFR3D.Infrastructure.Services.DocumentServices;
using VFR3D.Infrastructure.Services.WeatherServices;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("api.appsettings.json", optional: false)
    .AddJsonFile($"api.appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Setup CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5173",
                    "https://www.vfr3d.com",
                    "https://vfr3d.netlify.app")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Setup Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

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
    })
    .AddScheme<AuthenticationSchemeOptions, ConditionalAuthHandler>("Conditional", null);


// Setup Swagger
builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "VFR3D API";
    options.Version = "v1";
    
    // Add security definition
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
builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

// Setup DB Context
builder.Services.AddDbContext<VFR3DDbContext>((serviceProvider, options) =>
{
    var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    options.UseNpgsql(dbSettings.GetConnectionString(),
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
builder.Services.AddAwsServices(builder.Configuration);
builder.Services.AddScoped<IMetarService, MetarService>();
builder.Services.AddScoped<IPirepService, PirepService>();
builder.Services.AddScoped<ITafService, TafService>();
builder.Services.AddScoped<IAirsigmetService, AirsigmetService>();
builder.Services.AddScoped<IAirportDiagramService, AirportDiagramService>();
builder.Services.AddScoped<IChartSupplementService, ChartSupplementService>();  
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddScoped<ICommunicationFrequencyService, CommunicationFrequencyService>();
builder.Services.AddScoped<IAirspaceService, AirspaceService>();
builder.Services.AddScoped<IMagneticVariationService, MagneticVariationService>();
builder.Services.AddScoped<IWindsAloftService, WindsAloftService>();
builder.Services.AddScoped<INavlogService, NavlogService>();
builder.Services.AddScoped<IAircraftPerformanceProfileService, AircraftPerformanceProfileService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<ConditionalAuthHandler>();

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors("AllowedOrigins");

if(app.Environment.IsProduction())
{
    builder.Logging.AddAzureWebAppDiagnostics();
}

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
