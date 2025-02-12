namespace VFR3D.Infrastructure.Interfaces
{
    public interface IChartSupplementCronService
    {
        Task DownloadAndProcessChartSupplementsAsync(CancellationToken cancellationToken = default);
    }
}
