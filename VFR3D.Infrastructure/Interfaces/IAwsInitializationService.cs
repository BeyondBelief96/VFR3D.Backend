namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAwsInitializationService
    {
        Task InitializeAsync(CancellationToken cancellationToken);
    }
}
