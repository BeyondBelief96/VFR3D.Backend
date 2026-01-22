using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices;

namespace VFR3D.Azure.Functions.Functions
{
    public class AirportFunction
    {
        private readonly AirportCronService _airportService;
        private readonly RunwayCronService _runwayService;
        private readonly RunwayEndCronService _runwayEndService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<AirportFunction> _logger;

        public AirportFunction(
            AirportCronService airportService,
            RunwayCronService runwayService,
            RunwayEndCronService runwayEndService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _airportService = airportService ?? throw new ArgumentNullException(nameof(airportService));
            _runwayService = runwayService ?? throw new ArgumentNullException(nameof(runwayService));
            _runwayEndService = runwayEndService ?? throw new ArgumentNullException(nameof(runwayEndService));
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

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.NasrSubscription_Airport, currentDate))
                {
                    _logger.LogInformation("Starting airport data update process");

                    // Process airports first (APT_BASE.csv, APT_ATT.csv, APT_CON.csv)
                    await _airportService.DownloadAndProcessDataAsync(cancellationToken);
                    _logger.LogInformation("Airport base data processing completed");

                    // Process runways (APT_RWY.csv)
                    await _runwayService.DownloadAndProcessDataAsync(cancellationToken);
                    _logger.LogInformation("Runway data processing completed");

                    // Process runway ends (APT_RWY_END.csv)
                    await _runwayEndService.DownloadAndProcessDataAsync(cancellationToken);
                    _logger.LogInformation("Runway end data processing completed");

                    // Link runway ends to their parent runways
                    await _runwayEndService.LinkRunwayEndsToRunwaysAsync(cancellationToken);
                    _logger.LogInformation("Runway end linking completed");

                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.NasrSubscription_Airport, currentDate);
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
