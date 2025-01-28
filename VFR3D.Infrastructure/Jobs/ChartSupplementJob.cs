using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class ChartSupplementJob : CronJobService
    {
        private IServiceProvider _serviceProvider;

        public ChartSupplementJob(IServiceProvider serviceProvider, ILogger<ChartSupplementJob> logger) : base("*/2 * * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateAsyncScope();
                var publicationService = scope.ServiceProvider.GetRequiredService<IFaaPublicationCycleService>();
                var currentDate = DateTime.UtcNow;

                if(await publicationService.ShouldRunUpdateAsync(PublicationType.ChartSupplement, currentDate))
                {
                    var chartSupplementService = scope.ServiceProvider.GetRequiredService<IChartSupplementService>();
                    await chartSupplementService.DownloadAndProcessChartSupplementsAsync(cancellationToken);
                    await publicationService.UpdateLastSuccessfulRunAsync(PublicationType.ChartSupplement, currentDate);
                }
                else
                {
                    _logger.LogInformation("No chart supplement update needed at this time");
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Chart Supplement service");
            }
        }
    }
}
