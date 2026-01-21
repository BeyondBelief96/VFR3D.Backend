using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;
using VFR3D.Domain.Exceptions;

namespace VFR3D.Infrastructure.Services.DocumentServices;

public class ChartSupplementService : IChartSupplementService
{
    private readonly VFR3DDbContext _context;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<ChartSupplementService> _logger;
    private readonly string _containerName;

    public ChartSupplementService(
        VFR3DDbContext context,
        ICloudStorageService cloudStorageService,
        IOptions<CloudStorageSettings> cloudStorageSettings,
        ILogger<ChartSupplementService> logger)
    {
        _context = context;
        _cloudStorageService = cloudStorageService;
        _logger = logger;
        _containerName = cloudStorageSettings.Value.ChartSupplementsContainerName
            ?? throw new InvalidOperationException("CloudStorage:ChartSupplementsContainerName not configured");
    }

    private string StripIcaoPrefix(string airportCode)
    {
        airportCode = airportCode.ToUpper();
        
        // Return original code if it's 3 or fewer characters
        if (airportCode.Length <= 3)
            return airportCode;

        // If it starts with K, P, or H and is 4 characters long,
        // return just the last 3 characters
        if (airportCode.Length == 4 && 
            (airportCode.StartsWith("K") || 
             airportCode.StartsWith("P") || 
             airportCode.StartsWith("H")))
        {
            return airportCode[1..];
        }

        return airportCode;
    }
    
    /// <summary>
    /// The format of chart supplement keys in my S3 bucket has the file name capitalized and extension lowercase.
    /// </summary>
    /// <param name="fileName">The filename key stored in S3</param>
    /// <returns></returns>
    private string TransformFileName(string fileName)
    {
        // Split the filename into name and extension
        var nameParts = fileName.Split('.');
        if (nameParts.Length != 2)
        {
            // If there's no extension or multiple dots, return the original filename uppercased
            return fileName.ToUpper();
        }

        // Uppercase the filename part, keep the extension lowercase
        return $"{nameParts[0].ToUpper()}.{nameParts[1].ToLower()}";
    }

    public async Task<ChartSupplementUrlDto> GetChartSupplementUrlByAirportCode(string airportCode)
    {
        var strippedCode = StripIcaoPrefix(airportCode);
        _logger.LogInformation("Searching for chart supplement with code: {StrippedCode} (original: {OriginalCode})", 
            strippedCode, airportCode);

        var chartSupplement = await _context.ChartSupplements
            .FirstOrDefaultAsync(c => c.AirportCode == strippedCode);

        if (chartSupplement == null)
        {
            throw new ResourceNotFoundException($"Chart supplement not found for airport with code: {airportCode}");
        }

        if (string.IsNullOrEmpty(chartSupplement.FileName))
        {
            throw new ResourceNotFoundException($"No supplement file found for airport with code: {airportCode}");
        }

        try
        {
            var transformedFileName = TransformFileName(chartSupplement.FileName);
            var presignedUrl = await _cloudStorageService.GeneratePresignedUrlAsync(
                _containerName,
                transformedFileName,
                TimeSpan.FromHours(1));

            return new ChartSupplementUrlDto
            {
                PdfUrl = presignedUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pre-signed URL for chart supplement: {AirportCode}", airportCode);
            throw;
        }
    }
}