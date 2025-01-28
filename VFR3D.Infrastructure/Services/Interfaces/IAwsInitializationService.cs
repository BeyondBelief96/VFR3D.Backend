namespace VFR3D.Infrastructure.Services.Interfaces
{
    public interface IAwsInitializationService
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
