using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Services.CronJobServices;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class SpecialUseAirspaceJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public SpecialUseAirspaceJob(
            ILogger<SpecialUseAirspaceJob> logger,
            IServiceProvider serviceProvider)
            : base("0 0 * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting special use airspace update job");
                using var scope = _serviceProvider.CreateScope();
                var publicationService = scope.ServiceProvider.GetRequiredService<IFaaPublicationCycleService>();
                var currentDate = DateTime.UtcNow;

                if(await publicationService.ShouldRunUpdateAsync(PublicationType.Airspaces, currentDate))
                {
                    var service = scope.ServiceProvider.GetRequiredService<IAirspaceCronService<SpecialUseAirspace>>();
                    await service.UpdateAirspacesAsync(cancellationToken);
                    _logger.LogInformation("Completed special use airspace update job");
                }
                else
                {
                    _logger.LogInformation("No special use airspace update needed at this time");
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing special use airspace update job");
                throw;
            }
        }
    }
}
