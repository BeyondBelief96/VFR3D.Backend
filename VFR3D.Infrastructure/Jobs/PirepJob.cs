using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices;

namespace VFR3D.Infrastructure.Jobs
{
    public class PirepJob : CronJobService
    {
        private readonly IServiceProvider _serviceProvider;

        public PirepJob(
            ILogger<PirepJob> logger,
            IServiceProvider serviceProvider)
            : base("*/5 * * * *", TimeZoneInfo.Utc, logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteJob(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PirepJob running at: {time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var pirepService = scope.ServiceProvider.GetRequiredService<IAviationWeatherService<Pirep>>();
                await pirepService.PollWeatherDataAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing PIREP service");
            }
        }
    }
}
