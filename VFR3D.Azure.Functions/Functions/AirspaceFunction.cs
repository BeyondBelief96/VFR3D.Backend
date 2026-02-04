using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions.Functions
{
    public class AirspaceFunction
    {
        private readonly IAirspaceCronService<Airspace> _airspaceService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<AirspaceFunction> _logger;

        public AirspaceFunction(
            IAirspaceCronService<Airspace> airspaceService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _airspaceService = airspaceService ?? throw new ArgumentNullException(nameof(airspaceService));
            _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
            _logger = loggerFactory.CreateLogger<AirspaceFunction>();
        }

        [Function("AirspaceFunction")]
        public async Task Run([TimerTrigger("0 0 2 * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Airspace Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                var currentDate = DateTime.UtcNow;

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.Airspaces, currentDate))
                {
                    // Check if this is first-time initialization (staggered startup - 10 minute delay)
                    var publicationCycle = await _publicationService.GetPublicationCycleAsync(PublicationType.Airspaces);
                    if (publicationCycle?.LastSuccessfulUpdate.HasValue != true)
                    {
                        _logger.LogInformation("First-time initialization detected for Airspace data, waiting 10 minutes for staggered startup");
                        await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
                    }

                    _logger.LogInformation("Starting airspace update process");
                    await _airspaceService.UpdateAirspacesAsync(cancellationToken);
                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.Airspaces, currentDate);
                    _logger.LogInformation("Airspace update completed successfully");
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