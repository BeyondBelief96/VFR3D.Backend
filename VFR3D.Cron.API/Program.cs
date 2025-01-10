using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Configuration;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Jobs;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.Interfaces;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure environment specific settings
        builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(prefix: "VFR3D_")
            .AddUserSecrets<Program>(optional: true);

        // Add services to the container.

        builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));
        builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("Database"));
        builder.Services.AddScoped<IMetarService, MetarService>();
        builder.Services.AddSingleton<IAwsSecretsService, AwsSecretsService>();
        builder.Services.AddDbContext<CronServiceDbContext>((serviceProvider, options) =>
        {
            var dbSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

            options.UseNpgsql(dbSettings.GetConnectionString(),
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
        });

        builder.Services.AddHostedService<MetarJob>();

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