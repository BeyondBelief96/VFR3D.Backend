using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions.Functions
{
    public class ChartSupplementFunction
    {
        private readonly IChartSupplementCronService _chartSupplementService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<ChartSupplementFunction> _logger;

        public ChartSupplementFunction(
            IChartSupplementCronService chartSupplementService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _chartSupplementService = chartSupplementService ?? throw new ArgumentNullException(nameof(chartSupplementService));
            _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
            _logger = loggerFactory.CreateLogger<ChartSupplementFunction>();
        }

        [Function("ChartSupplementFunction")]
        public async Task Run([TimerTrigger("0 0 4 * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Chart Supplement Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                var currentDate = DateTime.UtcNow;

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.ChartSupplement, currentDate))
                {
                    // Check if this is first-time initialization (staggered startup - 20 minute delay)
                    var publicationCycle = await _publicationService.GetPublicationCycleAsync(PublicationType.ChartSupplement);
                    if (publicationCycle?.LastSuccessfulUpdate.HasValue != true)
                    {
                        _logger.LogInformation("First-time initialization detected for Chart Supplement data, waiting 20 minutes for staggered startup");
                        await Task.Delay(TimeSpan.FromMinutes(20), cancellationToken);
                    }

                    _logger.LogInformation("Starting chart supplement update process");
                    await _chartSupplementService.DownloadAndProcessChartSupplementsAsync(cancellationToken);
                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.ChartSupplement, currentDate);
                    _logger.LogInformation("Chart supplement update completed successfully");
                }
                else
                {
                    _logger.LogInformation("No chart supplement update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Chart Supplement service");
                throw;
            }
        }
    }
}