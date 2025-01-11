namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IAviationWeatherService<T>
    {
        Task PollWeatherDataAsync(CancellationToken cancellationToken = default);
    }
}
