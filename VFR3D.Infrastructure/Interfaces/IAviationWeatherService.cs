namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAviationWeatherService<T>
    {
        Task PollWeatherDataAsync(CancellationToken cancellationToken = default);
    }
}
