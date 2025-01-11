namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IPirepService
    {
        Task PollPirepApiAsync(CancellationToken cancellationToken = default);
    }
}
