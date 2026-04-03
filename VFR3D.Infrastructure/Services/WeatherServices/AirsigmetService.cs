using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices;

public class AirsigmetService : IAirsigmetService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<AirsigmetService> _logger;

    public AirsigmetService(
        VFR3DDbContext context,
        ILogger<AirsigmetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<AirsigmetDto>> GetAllAirsigmets()
    {
        try
        {
            var airsigmets = await _context.Airsigmets.ToListAsync();
            return airsigmets.Select(AirsigmetMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all AIRSIGMETs");
            throw;
        }
    }

    public async Task<IEnumerable<AirsigmetDto>> GetAirsigmetsByHazardType(AirsigmetHazardType hazardType)
    {
        try
        {
            var hazardTypeString = ConvertHazardTypeToString(hazardType);
            var airsigmets = await _context.Airsigmets
                .Where(a => a.Hazard != null && a.Hazard.Type == hazardTypeString)
                .ToListAsync();
            return airsigmets.Select(AirsigmetMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AIRSIGMETs for hazard type {HazardType}", hazardType);
            throw;
        }
    }

    private static string ConvertHazardTypeToString(AirsigmetHazardType hazardType)
    {
        return hazardType switch
        {
            AirsigmetHazardType.CONVECTIVE => "CONVECTIVE",
            AirsigmetHazardType.ICE => "ICE",
            AirsigmetHazardType.TURB => "TURB",
            AirsigmetHazardType.IFR => "IFR",
            AirsigmetHazardType.MTN_OBSCN => "MTN OBSCN",
            _ => throw new ArgumentOutOfRangeException(nameof(hazardType))
        };
    }
}