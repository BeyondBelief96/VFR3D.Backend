using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions.Functions
{
    public class SpecialUseAirspaceFunction
    {
        private readonly IAirspaceCronService<SpecialUseAirspace> _specialUseAirspaceService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<SpecialUseAirspaceFunction> _logger;

        public SpecialUseAirspaceFunction(
            IAirspaceCronService<SpecialUseAirspace> specialUseAirspaceService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _specialUseAirspaceService = specialUseAirspaceService ?? throw new ArgumentNullException(nameof(specialUseAirspaceService));
            _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
            _logger = loggerFactory.CreateLogger<SpecialUseAirspaceFunction>();
        }

        [Function("SpecialUseAirspaceFunction")]
        public async Task Run([TimerTrigger("0 0 3 * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Special Use Airspace Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                var currentDate = DateTime.UtcNow;

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.SpecialUseAirspaces, currentDate))
                {
                    // Check if this is first-time initialization (staggered startup - 15 minute delay)
                    var publicationCycle = await _publicationService.GetPublicationCycleAsync(PublicationType.SpecialUseAirspaces);
                    if (publicationCycle?.LastSuccessfulUpdate.HasValue != true)
                    {
                        _logger.LogInformation("First-time initialization detected for Special Use Airspace data, waiting 15 minutes for staggered startup");
                        await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
                    }

                    _logger.LogInformation("Starting special use airspace update process");
                    await _specialUseAirspaceService.UpdateAirspacesAsync(cancellationToken);
                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.SpecialUseAirspaces, currentDate);
                    _logger.LogInformation("Special use airspace update completed successfully");
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