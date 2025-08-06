using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices;

namespace VFR3D.Azure.Functions
{
    public class AirportFunction
    {
        private readonly AirportCronService _airportService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<AirportFunction> _logger;

        public AirportFunction(
            AirportCronService airportService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _airportService = airportService ?? throw new ArgumentNullException(nameof(airportService));
            _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
            _logger = loggerFactory.CreateLogger<AirportFunction>();
        }

        [Function("AirportFunction")]
        public async Task Run([TimerTrigger("0 0 * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Airport Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                var currentDate = DateTime.UtcNow;

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.NasrSubscription, currentDate))
                {
                    _logger.LogInformation("Starting airport data update process");
                    await _airportService.DownloadAndProcessDataAsync(cancellationToken);
                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.NasrSubscription, currentDate);
                    _logger.LogInformation("Airport data update completed successfully");
                }
                else
                {
                    _logger.LogInformation("No airport data update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating airport data");
                throw;
            }
        }
    }
}