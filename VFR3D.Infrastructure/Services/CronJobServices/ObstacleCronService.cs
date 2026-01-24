using System.Globalization;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services.CronJobServices;

public class ObstacleCronService : IObstacleCronService
{
    private readonly ILogger<ObstacleCronService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly VFR3DDbContext _dbContext;
    private readonly GeometryFactory _geometryFactory;

    private const int BatchSize = 5000;
    private const int HeaderLines = 4;

    public ObstacleCronService(
        ILogger<ObstacleCronService> logger,
        IHttpClientFactory httpClientFactory,
        VFR3DDbContext dbContext)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dbContext = dbContext;
        _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    }

    public async Task DownloadAndProcessObstaclesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var publicationCycle = await _dbContext.FaaPublicationCycles
                .FirstOrDefaultAsync(p => p.PublicationType == PublicationType.Obstacles, cancellationToken);

            if (publicationCycle == null)
            {
                throw new Exception("No publication cycle found for Obstacles.");
            }

            var currentPublicationDate = FaaPublicationDateUtils.CalculateCurrentPublicationDate(
                publicationCycle.KnownValidDate, publicationCycle.CycleLengthDays);
            var dateString = FaaPublicationDateUtils.FormatDateForObstacles(currentPublicationDate);
            var url = $"https://aeronav.faa.gov/Obst_Data/DOF_{dateString}.zip";

            _logger.LogInformation(
                "Starting obstacle download from URL: {Url} for publication date: {PublicationDate}",
                url,
                currentPublicationDate);

            using var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var zipArchive = new ZipArchive(stream);

            var obstacles = new List<Obstacle>();

            // Use DOF.DAT master file which contains all obstacles (no duplicates)
            var dofEntry = zipArchive.Entries.FirstOrDefault(e =>
                e.Name.Equals("DOF.DAT", StringComparison.OrdinalIgnoreCase));

            if (dofEntry == null)
            {
                throw new Exception("DOF.DAT master file not found in ZIP archive");
            }

            _logger.LogInformation("Processing master file: {FileName}", dofEntry.Name);

            using var entryStream = dofEntry.Open();
            using var reader = new StreamReader(entryStream);

            // Skip header lines
            for (int i = 0; i < HeaderLines; i++)
            {
                await reader.ReadLineAsync(cancellationToken);
            }

            string? line;
            var lineNumber = HeaderLines;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line) || line.Length < 100)
                {
                    continue;
                }

                var obstacle = ParseObstacleLine(line, dofEntry.Name, lineNumber);
                if (obstacle != null)
                {
                    obstacles.Add(obstacle);
                }

                // Log progress every 100,000 records
                if (lineNumber % 100000 == 0)
                {
                    _logger.LogInformation("Parsed {Count} obstacles so far...", obstacles.Count);
                }
            }

            _logger.LogInformation("Parsed {ObstacleCount} obstacles from DOF.DAT. Starting database update...",
                obstacles.Count);

            // Full refresh: delete all existing obstacles and bulk insert new ones
            await _dbContext.Obstacles.ExecuteDeleteAsync(cancellationToken);
            _logger.LogInformation("Deleted existing obstacles from database");

            // Batch insert
            for (int i = 0; i < obstacles.Count; i += BatchSize)
            {
                var batch = obstacles.Skip(i).Take(BatchSize).ToList();
                await _dbContext.Obstacles.AddRangeAsync(batch, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                _logger.LogInformation("Inserted batch {BatchNumber}/{TotalBatches} ({Count} obstacles)",
                    (i / BatchSize) + 1,
                    (obstacles.Count / BatchSize) + 1,
                    batch.Count);
            }

            _logger.LogInformation("Completed obstacle processing. Total obstacles: {Count}", obstacles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing obstacles");
            throw;
        }
    }

    private Obstacle? ParseObstacleLine(string line, string fileName, int lineNumber)
    {
        try
        {
            // Fixed-width parsing based on DOF format specification
            var oasCode = SafeSubstring(line, 0, 2).Trim();
            var obstacleNumber = SafeSubstring(line, 3, 6).Trim();
            var oasNumber = $"{oasCode}-{obstacleNumber}";

            var verificationStatus = SafeSubstring(line, 10, 1).Trim();
            var countryId = SafeSubstring(line, 12, 2).Trim();
            var stateId = SafeSubstring(line, 15, 2).Trim();
            var cityName = SafeSubstring(line, 18, 16).Trim();

            // Parse latitude (DMS)
            if (!int.TryParse(SafeSubstring(line, 35, 2).Trim(), out var latDeg))
            {
                return null;
            }
            if (!int.TryParse(SafeSubstring(line, 38, 2).Trim(), out var latMin))
            {
                return null;
            }
            if (!decimal.TryParse(SafeSubstring(line, 41, 5).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var latSec))
            {
                return null;
            }
            var latHemi = SafeSubstring(line, 46, 1).Trim();

            // Parse longitude (DMS)
            if (!int.TryParse(SafeSubstring(line, 48, 3).Trim(), out var longDeg))
            {
                return null;
            }
            if (!int.TryParse(SafeSubstring(line, 52, 2).Trim(), out var longMin))
            {
                return null;
            }
            if (!decimal.TryParse(SafeSubstring(line, 55, 5).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var longSec))
            {
                return null;
            }
            var longHemi = SafeSubstring(line, 60, 1).Trim();

            // Convert DMS to decimal
            var latDecimal = ConvertDmsToDecimal(latDeg, latMin, latSec, latHemi);
            var longDecimal = ConvertDmsToDecimal(longDeg, longMin, longSec, longHemi);

            var obstacleType = SafeSubstring(line, 62, 18).Trim();
            int.TryParse(SafeSubstring(line, 81, 1).Trim(), out var quantity);
            int.TryParse(SafeSubstring(line, 83, 5).Trim(), out var heightAgl);
            int.TryParse(SafeSubstring(line, 89, 5).Trim(), out var heightAmsl);

            var lighting = SafeSubstring(line, 95, 1).Trim();
            var horizontalAccuracy = SafeSubstring(line, 97, 1).Trim();
            var verticalAccuracy = SafeSubstring(line, 99, 1).Trim();
            var markIndicator = SafeSubstring(line, 101, 1).Trim();
            var faaStudyNumber = SafeSubstring(line, 103, 14).Trim();
            var action = SafeSubstring(line, 118, 1).Trim();
            var julianDate = line.Length >= 127 ? SafeSubstring(line, 120, 7).Trim() : null;

            return new Obstacle
            {
                OasNumber = oasNumber,
                OasCode = oasCode,
                ObstacleNumber = obstacleNumber,
                VerificationStatus = string.IsNullOrEmpty(verificationStatus) ? null : verificationStatus,
                CountryId = string.IsNullOrEmpty(countryId) ? null : countryId,
                StateId = string.IsNullOrEmpty(stateId) ? null : stateId,
                CityName = string.IsNullOrEmpty(cityName) ? null : cityName,
                LatDegrees = latDeg,
                LatMinutes = latMin,
                LatSeconds = latSec,
                LatHemisphere = latHemi,
                LongDegrees = longDeg,
                LongMinutes = longMin,
                LongSeconds = longSec,
                LongHemisphere = longHemi,
                LatDecimal = latDecimal,
                LongDecimal = longDecimal,
                ObstacleType = string.IsNullOrEmpty(obstacleType) ? null : obstacleType,
                Quantity = quantity > 0 ? quantity : null,
                HeightAgl = heightAgl > 0 ? heightAgl : null,
                HeightAmsl = heightAmsl > 0 ? heightAmsl : null,
                Lighting = string.IsNullOrEmpty(lighting) ? null : lighting,
                HorizontalAccuracy = string.IsNullOrEmpty(horizontalAccuracy) ? null : horizontalAccuracy,
                VerticalAccuracy = string.IsNullOrEmpty(verticalAccuracy) ? null : verticalAccuracy,
                MarkIndicator = string.IsNullOrEmpty(markIndicator) ? null : markIndicator,
                FaaStudyNumber = string.IsNullOrEmpty(faaStudyNumber) ? null : faaStudyNumber,
                Action = string.IsNullOrEmpty(action) ? null : action,
                JulianDate = string.IsNullOrEmpty(julianDate) ? null : julianDate,
                Location = _geometryFactory.CreatePoint(new Coordinate((double)longDecimal, (double)latDecimal))
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse obstacle line in {FileName} at line {LineNumber}: {Line}",
                fileName, lineNumber, line.Length > 50 ? line[..50] + "..." : line);
            return null;
        }
    }

    private static string SafeSubstring(string str, int startIndex, int length)
    {
        if (startIndex >= str.Length)
        {
            return string.Empty;
        }

        var actualLength = Math.Min(length, str.Length - startIndex);
        return str.Substring(startIndex, actualLength);
    }

    private static decimal ConvertDmsToDecimal(int degrees, int minutes, decimal seconds, string hemisphere)
    {
        var result = degrees + (minutes / 60.0m) + (seconds / 3600.0m);
        return (hemisphere == "S" || hemisphere == "W") ? -result : result;
    }
}
