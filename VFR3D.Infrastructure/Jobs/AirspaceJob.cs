using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Services.CronJobServices;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class AirspaceJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public AirspaceJob(
            ILogger<AirspaceJob> logger,
            IServiceProvider serviceProvider)
            : base("0 0 * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting airspace update job");
                using var scope = _serviceProvider.CreateScope();
                var publicationService = scope.ServiceProvider.GetRequiredService<IFaaPublicationCycleService>();
                var currentDate = DateTime.UtcNow;
                
                if(await publicationService.ShouldRunUpdateAsync(PublicationType.Airspaces, currentDate))
                {
                    var airspaceService = scope.ServiceProvider.GetRequiredService<IAirspaceService<Airspace>>();
                    await airspaceService.UpdateAirspacesAsync(cancellationToken);
                    _logger.LogInformation("Completed airspace update job");
                }
                else
                {
                    _logger.LogInformation("No airspace update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing airspace update job");
                throw;
            }
        }
    }

}
