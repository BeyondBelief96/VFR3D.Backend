using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices
{
    public class MetarService : IMetarService
    {
        private readonly VFR3DDbContext _context;
        private readonly ILogger<MetarService> _logger;

        public MetarService(
            VFR3DDbContext context,
            ILogger<MetarService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MetarDto> GetMetarForAirport(string icaoIdOrIdent)
        {
            var metar = await _context.Metars
                .FirstOrDefaultAsync(m => m.StationId == icaoIdOrIdent.ToUpper());

            if (metar == null)
            {
                var airport = await _context.Airports
                    .FirstOrDefaultAsync(a => a.ArptId == icaoIdOrIdent.ToUpper() ||
                                            a.IcaoId == icaoIdOrIdent.ToUpper());

                if (airport == null)
                {
                    throw new ResourceNotFoundException($"Airport not found for ICAO code or identifier: {icaoIdOrIdent}");
                }

                var modifiedIdent = airport.StateCode switch
                {
                    "AK" or "HI" => $"P{airport.ArptId}",
                    _ => $"K{airport.ArptId}"
                };

                metar = await _context.Metars
                    .FirstOrDefaultAsync(m => m.StationId == modifiedIdent);
            }

            if (metar == null)
            {
                throw new ResourceNotFoundException($"METAR not found for airport with ICAO ID: {icaoIdOrIdent}");
            }

            return MetarMapper.ToDto(metar);
        }

        public async Task<IEnumerable<MetarDto>> GetMetarsByState(string stateCode)
        {
            var upperStateCode = stateCode.ToUpper();

            var metars = await _context.Metars
                .Where(m => _context.Airports.Any(a =>
                    a.StateCode == upperStateCode &&
                    (a.IcaoId == m.StationId ||
                     (m.StationId != null &&
                      m.StationId.Length > 1 &&
                      (m.StationId.StartsWith("K") || m.StationId.StartsWith("P")) &&
                      a.ArptId == m.StationId.Substring(1)) ||
                     a.ArptId == m.StationId)))
                .ToListAsync();

            return metars.Select(MetarMapper.ToDto);
        }

        public async Task<IEnumerable<MetarDto>> GetMetarsByStates(string[] stateCodes)
        {
            var upperStateCodes = stateCodes.Select(s => s?.ToUpper()).Where(s => s != null).ToHashSet();

            var metars = await _context.Metars
                .Where(m => _context.Airports.Any(a =>
                    a.StateCode != null &&
                    upperStateCodes.Contains(a.StateCode) &&
                    (a.IcaoId == m.StationId ||
                     (m.StationId != null &&
                      m.StationId.Length > 1 &&
                      (m.StationId.StartsWith("K") || m.StationId.StartsWith("P")) &&
                      a.ArptId == m.StationId.Substring(1)) ||
                     a.ArptId == m.StationId)))
                .ToListAsync();

            return metars.Select(MetarMapper.ToDto);
        }
    }
}
