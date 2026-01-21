namespace VFR3D.Infrastructure.Interfaces;

/// <summary>
/// Cloud-agnostic storage service abstraction for blob/object storage operations.
/// </summary>
public interface ICloudStorageService
{
    /// <summary>
    /// Generates a pre-signed URL (AWS) or SAS token URL (Azure) for temporary blob access.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    /// <param name="blobName">The blob/object key</param>
    /// <param name="expiration">Time-to-live for the URL</param>
    /// <returns>A time-limited URL for direct blob access</returns>
    Task<string> GeneratePresignedUrlAsync(string containerName, string blobName, TimeSpan expiration);

    /// <summary>
    /// Uploads a blob to the specified container.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    /// <param name="blobName">The blob/object key</param>
    /// <param name="content">The stream containing blob data</param>
    /// <param name="contentType">The MIME type of the blob</param>
    Task UploadBlobAsync(string containerName, string blobName, Stream content, string contentType);

    /// <summary>
    /// Deletes a single blob from the container.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    /// <param name="blobName">The blob/object key to delete</param>
    Task DeleteBlobAsync(string containerName, string blobName);

    /// <summary>
    /// Deletes multiple blobs in a batch operation.
    /// Note: AWS S3 supports up to 1000 objects per batch, Azure Blob Storage supports up to 256.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    /// <param name="blobNames">Collection of blob/object keys to delete</param>
    Task DeleteBlobsAsync(string containerName, IEnumerable<string> blobNames);

    /// <summary>
    /// Checks if a container/bucket exists.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    /// <returns>True if the container exists, false otherwise</returns>
    Task<bool> ContainerExistsAsync(string containerName);

    /// <summary>
    /// Creates a new container/bucket.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    Task CreateContainerAsync(string containerName);

    /// <summary>
    /// Lists all blobs in a container, optionally filtered by prefix.
    /// </summary>
    /// <param name="containerName">The container/bucket name</param>
    /// <param name="prefix">Optional prefix to filter blob names</param>
    /// <returns>List of blob names</returns>
    Task<List<string>> ListBlobsAsync(string containerName, string? prefix = null);
}
