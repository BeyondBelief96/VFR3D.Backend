namespace VFR3D.Infrastructure.Interfaces
{
    public interface IChartSupplementService
    {
        Task DownloadAndProcessChartSupplementsAsync(CancellationToken cancellationToken = default);
    }
}
