using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
}