using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Services.NasrServices;
using VFR3D.Infrastructure.Services.CronJobServices;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Jobs
{
    public class FrequencyJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public FrequencyJob(
            IServiceProvider serviceProvider,
            ILogger<FrequencyJob> logger)
            : base("0 0 * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var publicationService = scope.ServiceProvider.GetRequiredService<IFaaPublicationCycleService>();
                var currentDate = DateTime.UtcNow;

                if (await publicationService.ShouldRunUpdateAsync(PublicationType.NasrSubscription, currentDate))
                {
                    var frequencyService = scope.ServiceProvider.GetRequiredService<FrequencyService>();
                    await frequencyService.DownloadAndProcessDataAsync(cancellationToken);
                    await publicationService.UpdateLastSuccessfulRunAsync(PublicationType.NasrSubscription, currentDate);
                }
                else
                {
                    _logger.LogInformation("No frequency data update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Frequency service");
            }
        }
    }
}
