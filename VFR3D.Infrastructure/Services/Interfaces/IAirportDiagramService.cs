namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IAirportDiagramService
    {
        Task DownloadAndProcessAirportDiagramsAsync(CancellationToken cancellationToken = default);
    }
}
