using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices;

public class PirepService : IPirepService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<PirepService> _logger;

    public PirepService(
        VFR3DDbContext context,
        ILogger<PirepService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PirepDto>> GetAllPireps()
    {
        try
        {
            var pireps = await _context.Pireps.ToListAsync();
            return pireps.Select(PirepMapper.ToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all PIREPs");
            throw;
        }
    }
}