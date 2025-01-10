namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IMetarService
    {
        Task PollMetarApiAsync(CancellationToken cancellationToken = default);
    }
}
