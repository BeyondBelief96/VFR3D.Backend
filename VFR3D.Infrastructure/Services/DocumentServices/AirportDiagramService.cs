using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;
using VFR3D.Domain.Exceptions;

namespace VFR3D.Infrastructure.Services.DocumentServices;

public class AirportDiagramService : IAirportDiagramService
{
    private readonly VFR3DDbContext _context;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly ILogger<AirportDiagramService> _logger;
    private readonly string _containerName;

    public AirportDiagramService(
        VFR3DDbContext context,
        ICloudStorageService cloudStorageService,
        IOptions<CloudStorageSettings> cloudStorageSettings,
        ILogger<AirportDiagramService> logger)
    {
        _context = context;
        _cloudStorageService = cloudStorageService;
        _logger = logger;
        _containerName = cloudStorageSettings.Value.AirportDiagramsContainerName
            ?? throw new InvalidOperationException("CloudStorage:AirportDiagramsContainerName not configured");
    }

    public async Task<AirportDiagramUrlDto> GetAirportDiagramUrlByAirportCode(string airportCode)
    {
        var airportDiagram = await _context.AirportDiagrams
            .FirstOrDefaultAsync(d => d.IcaoIdent == airportCode.ToUpper() || 
                                    d.AirportIdent == airportCode.ToUpper());

        if (airportDiagram == null)
        {
            throw new ResourceNotFoundException($"Airport diagram not found for airport with code: {airportCode}");
        }

        if (string.IsNullOrEmpty(airportDiagram.FileName))
        {
            throw new ResourceNotFoundException($"No diagram file found for airport with code: {airportCode}");
        }

        try
        {
            var transformedFileName = airportDiagram.FileName.ToUpper();
            var presignedUrl = await _cloudStorageService.GeneratePresignedUrlAsync(
                _containerName,
                transformedFileName,
                TimeSpan.FromHours(1));

            return new AirportDiagramUrlDto
            {
                PdfUrl = presignedUrl
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating pre-signed URL for airport diagram: {AirportCode}", airportCode);
            throw;
        }
    }
}