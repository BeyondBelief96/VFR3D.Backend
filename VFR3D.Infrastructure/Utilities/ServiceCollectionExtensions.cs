using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CloudStorage;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Utilities;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Azure Blob Storage services for cloud storage.
    /// </summary>
    public static IServiceCollection AddCloudStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Azure Blob Storage settings
        services.Configure<CloudStorageSettings>(configuration.GetSection("CloudStorage"));

        // Register Azure Blob Storage implementation
        services.AddScoped<ICloudStorageService, AzureBlobStorageService>();

        // Register initialization service
        services.AddScoped<ICloudStorageInitializationService, CloudStorageInitializationService>();

        return services;
    }
}
