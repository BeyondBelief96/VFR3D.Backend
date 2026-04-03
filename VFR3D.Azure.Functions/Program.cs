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
using VFR3D.Infrastructure.Services.CronJobServices;
using VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices;
using VFR3D.Infrastructure.Settings;
using VFR3D.Infrastructure.Utilities;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Configuration
    .SetBasePath(Path.GetDirectoryName(typeof(Program).Assembly.Location)!)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true, reloadOnChange: true);

// Add Application Insights early - before other service registrations
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure logging to ensure logs go to Application Insights
builder.Logging.AddApplicationInsights();

// Register settings
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));

// Register services
builder.Services.AddScoped<IFaaPublicationCycleService, FaaPublicationCycleService>();
builder.Services.AddScoped<IChartSupplementCronService, ChartSupplementCronService>();
builder.Services.AddScoped<IAirportDiagramCronService, AirportDiagramCronService>();
builder.Services.AddScoped<IAviationWeatherService<Metar>, MetarCronService>();
builder.Services.AddScoped<IAviationWeatherService<Taf>, TafCronService>();
builder.Services.AddScoped<IAviationWeatherService<Airsigmet>, AirsigmetCronService>();
builder.Services.AddScoped<IAviationWeatherService<GAirmet>, GAirmetCronService>();
builder.Services.AddScoped<IAviationWeatherService<Pirep>, PirepCronService>();
builder.Services.AddScoped<IAirspaceCronService<Airspace>, AirspaceCronService>();
builder.Services.AddScoped<IAirspaceCronService<SpecialUseAirspace>, SpecialUseAirspaceCronService>();
builder.Services.AddScoped<AirportCronService>();
builder.Services.AddScoped<CommunicationFrequencyCronService>();
builder.Services.AddScoped<RunwayCronService>();
builder.Services.AddScoped<RunwayEndCronService>();
builder.Services.AddScoped<IObstacleCronService, ObstacleCronService>();
builder.Services.AddCloudStorageServices(builder.Configuration);
builder.Services.AddHttpClient();

// Configure HttpClient for ArcGIS services with extended timeout
builder.Services.AddHttpClient("ArcGis", client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});

// Register database context
builder.Services.AddDbContext<VFR3DDbContext>((serviceProvider, options) =>
{
    var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    var connectionString = dbSettings.GetConnectionString();

    options.UseNpgsql(connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(3);
            npgsqlOptions.CommandTimeout(300); // 5 minutes for heavy NASR data operations
            npgsqlOptions.UseNetTopologySuite();
        });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Build the application
var app = builder.Build();

// Get logger from root provider (singletons are fine)
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Initialize Azure Blob Storage resources on startup (scoped service - needs scope)
logger.LogInformation("Initializing Azure Blob Storage resources during startup...");
try
{
    using (var scope = app.Services.CreateScope())
    {
        var cloudStorageInitService = scope.ServiceProvider.GetRequiredService<ICloudStorageInitializationService>();
        cloudStorageInitService.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
    logger.LogInformation("Azure Blob Storage resources initialized successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize Azure Blob Storage resources");
}

// Database seeding (scoped service - needs scope)
logger.LogInformation("Initializing database data...");
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<VFR3DDbContext>();
        DbInitializer.InitializeAsync(dbContext, logger).GetAwaiter().GetResult();
    }
    logger.LogInformation("Database initialization completed successfully");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to initialize database data");
}

app.Run();