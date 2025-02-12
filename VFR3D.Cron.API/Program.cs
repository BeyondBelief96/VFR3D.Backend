using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vfr3d.Domain.Entities;
using VFR3D.Cron.API.Extensions;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Jobs;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.ArcgisServices;
using VFR3D.Infrastructure.Services.CronJobServices;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices;
using VFR3D.Infrastructure.Settings;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "VFR3D_")
            .AddUserSecrets<Program>(optional: true);

        builder.Logging.AddAzureWebAppDiagnostics();

        builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));
        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
        builder.Services.AddScoped<IAviationWeatherService<Metar>, MetarCronService>();
        builder.Services.AddScoped<IAviationWeatherService<Taf>, TafCronService>();
        builder.Services.AddScoped<IAviationWeatherService<Pirep>, PirepCronService>();
        builder.Services.AddScoped<IAviationWeatherService<Airsigmet>, AirsigmetCronService>();
        builder.Services.AddScoped<IChartSupplementCronService, ChartSupplementCronCronService>();
        builder.Services.AddScoped<IAirportDiagramCronService, AirportDiagramCronCronService>();
        builder.Services.AddScoped<AirportService>();
        builder.Services.AddScoped<CommunicationFrequencyCronService>();
        builder.Services.AddScoped<IAirspaceCronService<Airspace>, AirspaceCronService>();
        builder.Services.AddScoped<IAirspaceCronService<SpecialUseAirspace>, SpecialUseAirspaceCronService>();
        builder.Services.AddScoped<IFaaPublicationCycleService, FaaPublicationCycleService>();
        builder.Services.AddSingleton<IAwsSecretsService, AwsSecretsService>();
        builder.Services.AddScoped<IAwsInitializationService, AwsInitializationService>();
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
        builder.Services.AddHostedService<AwsInitializationJob>();
        builder.Services.AddHostedService<MetarJob>();
        builder.Services.AddHostedService<TafJob>();
        builder.Services.AddHostedService<PirepJob>();
        builder.Services.AddHostedService<AirsigmetJob>();
        builder.Services.AddHostedService<ChartSupplementJob>();
        builder.Services.AddHostedService<AirportDiagramJob>();
        builder.Services.AddHostedService<AirportsJob>();
        builder.Services.AddHostedService<FrequencyJob>();
        builder.Services.AddHostedService<AirspaceJob>();
        builder.Services.AddHostedService<SpecialUseAirspaceJob>();
        builder.Services.AddAwsServices(builder.Configuration);
        builder.Services.AddHttpClient();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}