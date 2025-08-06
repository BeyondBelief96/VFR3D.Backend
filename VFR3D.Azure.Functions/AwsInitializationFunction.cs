using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions
{
    public class AwsInitializationFunction
    {
        private readonly IAwsInitializationService _awsInitializationService;
        private readonly ILogger<AwsInitializationFunction> _logger;

        public AwsInitializationFunction(
            IAwsInitializationService awsInitializationService,
            ILoggerFactory loggerFactory)
        {
            _awsInitializationService = awsInitializationService ?? throw new ArgumentNullException(nameof(awsInitializationService));
            _logger = loggerFactory.CreateLogger<AwsInitializationFunction>();
        }

        [Function("AwsInitializationFunction")]
        public async Task Run([TimerTrigger("0 */6 * * *", RunOnStartup = true), ] TimerInfo myTimer, FunctionContext context)
        {
            _logger.LogInformation($"AWS Initialization Function executed at: {DateTime.UtcNow}");
            var cancellationToken = context.CancellationToken;

            try
            {
                _logger.LogInformation("Starting AWS initialization");
                await _awsInitializationService.InitializeAsync(cancellationToken);
                _logger.LogInformation("AWS initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AWS initialization");
                throw;
            }
        }
    }
}