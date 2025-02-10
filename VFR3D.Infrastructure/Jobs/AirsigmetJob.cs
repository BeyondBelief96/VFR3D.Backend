using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices;

namespace VFR3D.Infrastructure.Jobs
{
    public class AirsigmetJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public AirsigmetJob(
            ILogger<AirsigmetJob> logger,
            IServiceProvider serviceProvider)
            : base("*/30 * * * *", TimeZoneInfo.Utc, logger) 
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            _logger.LogInformation("AirsigmetWorker running at: {time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var airsigmetService = scope.ServiceProvider.GetRequiredService<IAviationWeatherService<Airsigmet>>();
                await airsigmetService.PollWeatherDataAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing AIRSIGMET service");
            }
        }
    }
}
