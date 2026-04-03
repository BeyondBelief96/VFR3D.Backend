namespace VFR3D.Infrastructure.Interfaces;

/// <summary>
/// Service responsible for initializing cloud storage resources on application startup.
/// Ensures required containers/buckets exist.
/// </summary>
public interface ICloudStorageInitializationService
{
    /// <summary>
    /// Initializes cloud storage by creating required containers/buckets if they don't exist.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
