namespace VFR3D.Infrastructure.Settings;

/// <summary>
/// Configuration settings for Azure Blob Storage.
/// </summary>
public class CloudStorageSettings
{
    /// <summary>
    /// Azure Storage account name (e.g., "vfr3dstorage").
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Azure Storage account key. Only required if not using Managed Identity.
    /// </summary>
    public string? AccountKey { get; set; }

    /// <summary>
    /// Full connection string for Azure Storage.
    /// Used for local development with Azurite or when not using Managed Identity.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Container name for chart supplement PDFs.
    /// </summary>
    public string ChartSupplementsContainerName { get; set; } = "chart-supplements";

    /// <summary>
    /// Container name for airport diagram PDFs.
    /// </summary>
    public string AirportDiagramsContainerName { get; set; } = "airport-diagrams";

    /// <summary>
    /// Container name for user-uploaded aircraft documents.
    /// </summary>
    public string AircraftDocumentsContainerName { get; set; } = "aircraft-documents";

    /// <summary>
    /// Use Azure Managed Identity for authentication instead of explicit credentials.
    /// Recommended for production environments.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = false;
}
