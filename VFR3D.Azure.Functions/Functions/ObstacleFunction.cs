using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Azure.Functions.Functions;

public class ObstacleFunction
{
    private readonly IObstacleCronService _obstacleService;
    private readonly IFaaPublicationCycleService _publicationService;
    private readonly ILogger<ObstacleFunction> _logger;

    public ObstacleFunction(
        IObstacleCronService obstacleService,
        IFaaPublicationCycleService publicationService,
        ILoggerFactory loggerFactory)
    {
        _obstacleService = obstacleService ?? throw new ArgumentNullException(nameof(obstacleService));
        _publicationService = publicationService ?? throw new ArgumentNullException(nameof(publicationService));
        _logger = loggerFactory.CreateLogger<ObstacleFunction>();
    }

    [Function("ObstacleFunction")]
    public async Task Run([TimerTrigger("0 0 6 * * *", RunOnStartup = true)] TimerInfo myTimer, FunctionContext context)
    {
        _logger.LogInformation("Obstacle Function executed at: {Time}", DateTime.UtcNow);
        var cancellationToken = context.CancellationToken;

        try
        {
            var currentDate = DateTime.UtcNow;

            if (await _publicationService.ShouldRunUpdateAsync(PublicationType.Obstacles, currentDate))
            {
                _logger.LogInformation("Starting obstacle data update process");
                await _obstacleService.DownloadAndProcessObstaclesAsync(cancellationToken);
                await _publicationService.UpdateLastSuccessfulRunAsync(PublicationType.Obstacles, currentDate);
                _logger.LogInformation("Obstacle data update completed successfully");
            }
            else
            {
                _logger.LogInformation("No obstacle data update needed at this time");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing Obstacle service");
            throw;
        }
    }
}
