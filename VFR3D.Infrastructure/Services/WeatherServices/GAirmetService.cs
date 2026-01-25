using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices;

public class GAirmetService : IGAirmetService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<GAirmetService> _logger;

    public GAirmetService(
        VFR3DDbContext context,
        ILogger<GAirmetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<GAirmetDto>> GetAllGAirmets()
    {
        try
        {
            var gairmets = await _context.GAirmets.ToListAsync();
            return gairmets.Select(GAirmetMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all G-AIRMETs");
            throw;
        }
    }

    public async Task<IEnumerable<GAirmetDto>> GetGAirmetsByProduct(GAirmetProduct product)
    {
        try
        {
            var productString = product.ToString().ToUpperInvariant();
            var gairmets = await _context.GAirmets
                .Where(g => g.Product == productString)
                .ToListAsync();
            return gairmets.Select(GAirmetMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving G-AIRMETs for product {Product}", product);
            throw;
        }
    }

    public async Task<IEnumerable<GAirmetDto>> GetGAirmetsByHazardType(GAirmetHazardType hazardType)
    {
        try
        {
            var hazardTypeStrings = GetHazardTypeStrings(hazardType);
            var gairmets = await _context.GAirmets
                .Where(g => g.HazardType != null && hazardTypeStrings.Contains(g.HazardType))
                .ToListAsync();
            return gairmets.Select(GAirmetMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving G-AIRMETs for hazard type {HazardType}", hazardType);
            throw;
        }
    }

    private static List<string> GetHazardTypeStrings(GAirmetHazardType hazardType)
    {
        // Return list to handle cases where multiple string formats map to same enum
        return hazardType switch
        {
            GAirmetHazardType.MT_OBSC => ["MT_OBSC"],
            GAirmetHazardType.IFR => ["IFR"],
            GAirmetHazardType.TURB_LO => ["TURB-LO"],
            GAirmetHazardType.TURB_HI => ["TURB-HI"],
            GAirmetHazardType.LLWS => ["LLWS"],
            GAirmetHazardType.SFC_WIND => ["SFC_WIND", "SFC-WIND"], // Handle both formats
            GAirmetHazardType.ICE => ["ICE"],
            GAirmetHazardType.FZLVL => ["FZLVL"],
            GAirmetHazardType.M_FZLVL => ["M_FZLVL"],
            _ => []
        };
    }
}
