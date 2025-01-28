using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Services.Interfaces;

namespace VFR3D.Infrastructure.Services
{
    public class AwsInitializationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AwsInitializationHostedService> _logger;

        public AwsInitializationHostedService(
            IServiceProvider serviceProvider,
            ILogger<AwsInitializationHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting AWS initialization");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var initService = scope.ServiceProvider.GetRequiredService<IAwsInitializationService>();
                await initService.InitializeAsync(cancellationToken);
                _logger.LogInformation("AWS initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AWS initialization");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
