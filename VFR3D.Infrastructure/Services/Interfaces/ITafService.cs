namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface ITafService
    {
        Task PollTafApiAsync(CancellationToken cancellationToken = default);
    }
}
