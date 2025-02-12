using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services.DocumentServices;

public class AirportDiagramService : IAirportDiagramService
{
    private readonly VFR3DDbContext _context;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<AirportDiagramService> _logger;
    private readonly string _bucketName;

    public AirportDiagramService(
        VFR3DDbContext context,
        IAmazonS3 s3Client,
        IOptions<AwsSettings> awsSettings,
        ILogger<AirportDiagramService> logger)
    {
        _context = context;
        _s3Client = s3Client;
        _logger = logger;
        _bucketName = awsSettings.Value.AirportDiagramsBucketName 
            ?? throw new InvalidOperationException("AWS:AirportDiagramsBucketName not configured");
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
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = transformedFileName,
                Expires = DateTime.UtcNow.AddHours(1)
            };

            var presignedUrl = await Task.Run(() => _s3Client.GetPreSignedURL(request));

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