using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Services;
using VFR3D.Infrastructure.Services.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class AirportDiagramJob : CronJobService
    {
        private IServiceProvider _serviceProvider;

        public AirportDiagramJob(IServiceProvider serviceProvider, ILogger<ChartSupplementJob> logger) : base("*/10 * * * *", TimeZoneInfo.Utc, logger)
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

                if (await publicationService.ShouldRunUpdateAsync(PublicationType.AirportDiagram, currentDate))
                {
                    var airportDiagramService = scope.ServiceProvider.GetRequiredService<IAirportDiagramService>();
                    await airportDiagramService.DownloadAndProcessAirportDiagramsAsync(cancellationToken);
                    await publicationService.UpdateLastSuccessfulRunAsync(PublicationType.AirportDiagram, currentDate);
                }
                else
                {
                    _logger.LogInformation("No chart supplement update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Chart Supplement service");
            }
        }
    }
}
