namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAirportDiagramService
    {
        Task DownloadAndProcessAirportDiagramsAsync(CancellationToken cancellationToken = default);
    }
}
