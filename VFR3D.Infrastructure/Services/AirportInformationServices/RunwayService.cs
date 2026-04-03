using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.AirportInformationServices;

public class RunwayService : IRunwayService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<RunwayService> _logger;

    public RunwayService(
        VFR3DDbContext context,
        ILogger<RunwayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<RunwayDto>> GetRunwaysByAirportAsync(string icaoCodeOrIdent)
    {
        try
        {
            _logger.LogInformation("Getting runways for airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);

            // First find the airport to get its SiteNo
            var airport = await _context.Airports
                .FirstOrDefaultAsync(a =>
                    a.IcaoId == icaoCodeOrIdent.ToUpper() ||
                    a.ArptId == icaoCodeOrIdent.ToUpper());

            if (airport == null)
            {
                throw new ResourceNotFoundException(
                    $"Airport not found for ICAO code or identifier: {icaoCodeOrIdent}");
            }

            // Get all runways for this airport with their runway ends
            var runways = await _context.Runways
                .Include(r => r.RunwayEnds)
                .Where(r => r.SiteNo == airport.SiteNo)
                .OrderBy(r => r.RunwayId)
                .ToListAsync();

            return runways.Select(RunwayMapper.ToDto);
        }
        catch (Exception ex) when (ex is not ResourceNotFoundException)
        {
            _logger.LogError(ex, "Error getting runways for airport: {IcaoCodeOrIdent}", icaoCodeOrIdent);
            throw;
        }
    }
}
