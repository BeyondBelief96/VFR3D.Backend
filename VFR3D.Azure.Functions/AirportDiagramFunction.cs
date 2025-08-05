using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions
{
    public class AirportDiagramFunction
    {
        private readonly IAirportDiagramCronService _airportDiagramService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<AirportDiagramFunction> _logger;

        public AirportDiagramFunction(
            IAirportDiagramCronService airportDiagramService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _airportDiagramService = airportDiagramService ?? throw new ArgumentNullException(nameof(airportDiagramService));
            _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
            _logger = loggerFactory.CreateLogger<AirportDiagramFunction>();
        }

        [Function("AirportDiagramFunction")]
        public async Task Run([TimerTrigger("0 0 * * *")] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Airport Diagram Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                var currentDate = DateTime.UtcNow;

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.AirportDiagram, currentDate))
                {
                    _logger.LogInformation("Starting airport diagram update process");
                    await _airportDiagramService.DownloadAndProcessAirportDiagramsAsync(cancellationToken);
                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.AirportDiagram, currentDate);
                    _logger.LogInformation("Airport diagram update completed successfully");
                }
                else
                {
                    _logger.LogInformation("No airport diagram update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Airport Diagram service");
                throw;
            }
        }
    }
}