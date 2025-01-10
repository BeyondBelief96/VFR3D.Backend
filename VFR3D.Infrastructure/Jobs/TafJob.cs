using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class TafJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public TafJob(ILogger<TafJob> logger, IServiceProvider serviceProvider) : base("*/30 * * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MetarWorker running at: {time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metarService = scope.ServiceProvider.GetRequiredService<ITafService>();
                await metarService.PollTafApiAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while running METAR service");
            }
        }
    }
}
