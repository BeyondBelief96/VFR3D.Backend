using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices;

namespace VFR3D.Azure.Functions.Functions
{
    public class FrequencyFunction
    {
        private readonly CommunicationFrequencyCronService _frequencyService;
        private readonly IFaaPublicationCycleService _publicationService;
        private readonly ILogger<FrequencyFunction> _logger;

        public FrequencyFunction(
            CommunicationFrequencyCronService frequencyService,
            IFaaPublicationCycleService publicationService,
            ILoggerFactory loggerFactory)
        {
            _frequencyService = frequencyService ?? throw new ArgumentNullException(nameof(frequencyService));
            _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
            _logger = loggerFactory.CreateLogger<FrequencyFunction>();
        }

        [Function("FrequencyFunction")]
        public async Task Run([TimerTrigger("0 0 1 * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"Frequency Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                var currentDate = DateTime.UtcNow;

                if (await _publicationService.ShouldRunUpdateAsync(PublicationType.NasrSubscription_Frequencies, currentDate))
                {
                    _logger.LogInformation("Starting frequency data update process");
                    await _frequencyService.DownloadAndProcessDataAsync(cancellationToken);
                    await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.NasrSubscription_Frequencies, currentDate);
                    _logger.LogInformation("Frequency data update completed successfully");
                }
                else
                {
                    _logger.LogInformation("No frequency data update needed at this time");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing Frequency service");
                throw;
            }
        }
    }
}