using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services.CloudStorage;

/// <summary>
/// Cloud-agnostic storage initialization service.
/// Creates required containers on application startup.
/// </summary>
public class CloudStorageInitializationService : ICloudStorageInitializationService
{
    private readonly ICloudStorageService _storageService;
    private readonly ILogger<CloudStorageInitializationService> _logger;
    private readonly CloudStorageSettings _settings;

    public CloudStorageInitializationService(
        ICloudStorageService storageService,
        IOptions<CloudStorageSettings> cloudStorageSettings,
        ILogger<CloudStorageInitializationService> logger)
    {
        _storageService = storageService;
        _logger = logger;
        _settings = cloudStorageSettings.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing Azure Blob Storage...");

            // Ensure chart supplements container exists
            if (!await _storageService.ContainerExistsAsync(_settings.ChartSupplementsContainerName))
            {
                _logger.LogInformation("Creating chart supplements container: {ContainerName}",
                    _settings.ChartSupplementsContainerName);
                await _storageService.CreateContainerAsync(_settings.ChartSupplementsContainerName);
            }
            else
            {
                _logger.LogInformation("Chart supplements container already exists: {ContainerName}",
                    _settings.ChartSupplementsContainerName);
            }

            // Ensure airport diagrams container exists
            if (!await _storageService.ContainerExistsAsync(_settings.AirportDiagramsContainerName))
            {
                _logger.LogInformation("Creating airport diagrams container: {ContainerName}",
                    _settings.AirportDiagramsContainerName);
                await _storageService.CreateContainerAsync(_settings.AirportDiagramsContainerName);
            }
            else
            {
                _logger.LogInformation("Airport diagrams container already exists: {ContainerName}",
                    _settings.AirportDiagramsContainerName);
            }

            _logger.LogInformation("Azure Blob Storage initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure Blob Storage");
            throw;
        }
    }
}
