using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vfr3d.Domain.Entities;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.ArcgisServices;
using VFR3D.Infrastructure.Services.CronJobServices;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Register settings with explicit binding
builder.Services.Configure<DatabaseSettings>(options =>
{
    options.Host = builder.Configuration["Database:Host"] ??
                  builder.Configuration["Database__Host"] ??
                  "localhost";

    options.DatabaseName = builder.Configuration["Database:Database"] ??
                          builder.Configuration["Database__Database"] ??
                          "postgres";

    options.Username = builder.Configuration["Database:Username"] ??
                      builder.Configuration["Database__Username"] ??
                      "postgres";

    options.Password = builder.Configuration["Database:Password"] ??
                      builder.Configuration["Database__Password"] ??
                      string.Empty;

    if (int.TryParse(builder.Configuration["Database:Port"] ?? builder.Configuration["Database__Port"], out int port))
    {
        options.Port = port;
    }
    else
    {
        options.Port = 5432;
    }
});

// Register settings
builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));

// Register services
builder.Services.AddScoped<IAwsInitializationService, AwsInitializationService>();
builder.Services.AddScoped<IFaaPublicationCycleService, FaaPublicationCycleService>();
builder.Services.AddScoped<IChartSupplementCronService, ChartSupplementCronService>();
builder.Services.AddScoped<IAirportDiagramCronService, AirportDiagramCronCronService>();
builder.Services.AddScoped<IAviationWeatherService<Metar>, MetarCronService>();
builder.Services.AddScoped<IAviationWeatherService<Taf>, TafCronService>();
builder.Services.AddScoped<IAviationWeatherService<Airsigmet>, AirsigmetCronService>();
builder.Services.AddScoped<IAviationWeatherService<Pirep>, PirepCronService>();
builder.Services.AddScoped<IAirspaceCronService<Airspace>, AirspaceCronService>();
builder.Services.AddScoped<IAirspaceCronService<SpecialUseAirspace>, SpecialUseAirspaceCronService>();
builder.Services.AddScoped<AirportCronService>();
builder.Services.AddScoped<CommunicationFrequencyCronService>();
builder.Services.AddAwsServices(builder.Configuration);
builder.Services.AddHttpClient();

// Register database context
builder.Services.AddDbContext<VFR3DDbContext>((serviceProvider, options) =>
{
    var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    var connectionString = dbSettings.GetConnectionString();

    options.UseNpgsql(connectionString,
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
});

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Get required services for initialization
var serviceProvider = builder.Services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var awsInitService = serviceProvider.GetRequiredService<IAwsInitializationService>();

// Initialize AWS resources on startup
logger.LogInformation("Initializing AWS resources during startup...");
try
{
    // Run synchronously to ensure completion before functions start
    awsInitService.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    logger.LogInformation("AWS resources initialized successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize AWS resources");
    // Consider whether to throw and prevent startup or just log the error
}


builder.Build().Run();