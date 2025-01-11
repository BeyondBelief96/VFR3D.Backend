using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class MetarJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public MetarJob(ILogger<MetarJob> logger, IServiceProvider serviceProvider) : base("*/10 * * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MetarWorker running at: {time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metarService = scope.ServiceProvider.GetRequiredService<IAviationWeatherService<MetarJob>>();
                await metarService.PollWeatherDataAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running METAR service");
            }
        }
    }
}
