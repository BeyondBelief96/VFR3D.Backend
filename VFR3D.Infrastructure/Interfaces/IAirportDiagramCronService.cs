namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAirportDiagramCronService
    {
        Task DownloadAndProcessAirportDiagramsAsync(CancellationToken cancellationToken = default);
    }
}
