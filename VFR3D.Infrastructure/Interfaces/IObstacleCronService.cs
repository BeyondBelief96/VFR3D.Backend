namespace VFR3D.Infrastructure.Interfaces;

public interface IObstacleCronService
{
    Task DownloadAndProcessObstaclesAsync(CancellationToken cancellationToken = default);
}
