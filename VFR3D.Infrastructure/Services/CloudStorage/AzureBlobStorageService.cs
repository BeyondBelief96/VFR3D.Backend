using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services.CloudStorage;

/// <summary>
/// Azure Blob Storage implementation of cloud storage service.
/// Supports both connection string authentication (local development with Azurite)
/// and Managed Identity authentication (production).
/// </summary>
public class AzureBlobStorageService : ICloudStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly CloudStorageSettings _settings;

    public AzureBlobStorageService(
        IOptions<CloudStorageSettings> cloudStorageSettings,
        ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        _settings = cloudStorageSettings.Value;

        // Initialize BlobServiceClient based on configuration
        if (!string.IsNullOrEmpty(_settings.ConnectionString))
        {
            // Use connection string (local development with Azurite)
            _blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
            _logger.LogInformation("Azure Blob Storage initialized with connection string");
        }
        else if (_settings.UseManagedIdentity)
        {
            // Use Managed Identity (production)
            var credential = new DefaultAzureCredential();
            var serviceUri = new Uri($"https://{_settings.AccountName}.blob.core.windows.net");
            _blobServiceClient = new BlobServiceClient(serviceUri, credential);
            _logger.LogInformation("Azure Blob Storage initialized with Managed Identity for account: {AccountName}",
                _settings.AccountName);
        }
        else if (!string.IsNullOrEmpty(_settings.AccountKey))
        {
            // Use account key (fallback)
            var credential = new Azure.Storage.StorageSharedKeyCredential(_settings.AccountName, _settings.AccountKey);
            var serviceUri = new Uri($"https://{_settings.AccountName}.blob.core.windows.net");
            _blobServiceClient = new BlobServiceClient(serviceUri, credential);
            _logger.LogInformation("Azure Blob Storage initialized with account key for account: {AccountName}",
                _settings.AccountName);
        }
        else
        {
            throw new InvalidOperationException(
                "Azure Storage configuration is invalid. Provide either ConnectionString, UseManagedIdentity=true, or AccountKey.");
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string containerName, string blobName, TimeSpan expiration)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if blob exists
            if (!await blobClient.ExistsAsync())
            {
                throw new FileNotFoundException($"Blob '{blobName}' not found in container '{containerName}'");
            }

            // For connection string mode (Azurite), generate SAS token
            if (!string.IsNullOrEmpty(_settings.ConnectionString))
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b", // "b" for blob
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Allow 5 minutes clock skew
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiration)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                _logger.LogDebug("Generated SAS URL for blob: {BlobName} in container: {ContainerName}, expires in {Expiration}",
                    blobName, containerName, expiration);

                return sasUri.ToString();
            }
            else
            {
                // For Managed Identity or account key, create user delegation SAS or account SAS
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiration)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                Uri sasUri;

                if (_settings.UseManagedIdentity)
                {
                    // Generate user delegation SAS for Managed Identity
                    var userDelegationKey = await _blobServiceClient.GetUserDelegationKeyAsync(
                        startsOn: DateTimeOffset.UtcNow.AddMinutes(-5),
                        expiresOn: DateTimeOffset.UtcNow.Add(expiration));

                    var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
                    {
                        Sas = sasBuilder.ToSasQueryParameters(userDelegationKey.Value, _blobServiceClient.AccountName)
                    };
                    sasUri = blobUriBuilder.ToUri();
                }
                else
                {
                    // Generate account SAS for account key authentication
                    sasUri = blobClient.GenerateSasUri(sasBuilder);
                }

                _logger.LogDebug("Generated SAS URL for blob: {BlobName} in container: {ContainerName}, expires in {Expiration}",
                    blobName, containerName, expiration);

                return sasUri.ToString();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SAS URL for blob: {BlobName} in container: {ContainerName}",
                blobName, containerName);
            throw;
        }
    }

    public async Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            _logger.LogDebug("Uploaded blob: {BlobName} to container: {ContainerName}", blobName, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob: {BlobName} to container: {ContainerName}",
                blobName, containerName);
            throw;
        }
    }

    public async Task UploadBlobsAsync(string containerName, IEnumerable<(string BlobName, byte[] Content, string ContentType)> blobs, int maxConcurrency = 10)
    {
        var blobsList = blobs.ToList();
        if (blobsList.Count == 0)
        {
            return;
        }

        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var totalUploaded = 0;

            // Use SemaphoreSlim to limit concurrency
            using var semaphore = new SemaphoreSlim(maxConcurrency);
            var uploadTasks = blobsList.Select(async blob =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var blobClient = containerClient.GetBlobClient(blob.BlobName);
                    var blobHttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = blob.ContentType
                    };

                    using var stream = new MemoryStream(blob.Content);
                    await blobClient.UploadAsync(stream, new BlobUploadOptions
                    {
                        HttpHeaders = blobHttpHeaders
                    });

                    Interlocked.Increment(ref totalUploaded);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(uploadTasks);

            _logger.LogInformation("Uploaded {Count} blobs to container: {ContainerName}",
                totalUploaded, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blobs to container: {ContainerName}", containerName);
            throw;
        }
    }

    public async Task DeleteBlobAsync(string containerName, string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();

            _logger.LogDebug("Deleted blob: {BlobName} from container: {ContainerName}", blobName, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blob: {BlobName} from container: {ContainerName}",
                blobName, containerName);
            throw;
        }
    }

    public async Task DeleteBlobsAsync(string containerName, IEnumerable<string> blobNames)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobNamesList = blobNames.ToList();

            // Azure Blob Storage batch delete supports up to 256 blobs per batch
            const int batchSize = 256;
            var totalDeleted = 0;

            for (int i = 0; i < blobNamesList.Count; i += batchSize)
            {
                var batch = blobNamesList.Skip(i).Take(batchSize).ToList();

                // Delete blobs in this batch
                var deleteTasks = batch.Select(async blobName =>
                {
                    var blobClient = containerClient.GetBlobClient(blobName);
                    await blobClient.DeleteIfExistsAsync();
                });

                await Task.WhenAll(deleteTasks);
                totalDeleted += batch.Count;

                _logger.LogDebug("Deleted batch of {Count} blobs from container: {ContainerName} (total: {TotalDeleted}/{TotalBlobs})",
                    batch.Count, containerName, totalDeleted, blobNamesList.Count);
            }

            _logger.LogInformation("Deleted {Count} blobs from container: {ContainerName}",
                totalDeleted, containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting blobs from container: {ContainerName}", containerName);
            throw;
        }
    }

    public async Task<bool> ContainerExistsAsync(string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var exists = await containerClient.ExistsAsync();

            _logger.LogDebug("Container '{ContainerName}' exists: {Exists}", containerName, exists.Value);
            return exists.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if container exists: {ContainerName}", containerName);
            throw;
        }
    }

    public async Task CreateContainerAsync(string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create container with private access (no public access)
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            _logger.LogInformation("Created container: {ContainerName}", containerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container: {ContainerName}", containerName);
            throw;
        }
    }

    public async Task<List<string>> ListBlobsAsync(string containerName, string? prefix = null)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobNames = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobNames.Add(blobItem.Name);
            }

            _logger.LogDebug("Listed {Count} blobs from container: {ContainerName} with prefix: {Prefix}",
                blobNames.Count, containerName, prefix ?? "(none)");

            return blobNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs from container: {ContainerName}", containerName);
            throw;
        }
    }
}
