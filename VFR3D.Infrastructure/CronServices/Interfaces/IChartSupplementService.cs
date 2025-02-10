namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IChartSupplementService
    {
        Task DownloadAndProcessChartSupplementsAsync(CancellationToken cancellationToken = default);
    }
}
