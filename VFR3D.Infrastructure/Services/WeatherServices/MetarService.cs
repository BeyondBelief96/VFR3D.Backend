using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Exceptions;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MetarDto> GetMetarForAirport(string icaoIdOrIdent)
        {
            try
            {
                _logger.LogInformation("Retrieving METAR for airport identifier: {IcaoIdOrIdent}", icaoIdOrIdent);

                if (string.IsNullOrWhiteSpace(icaoIdOrIdent))
                {
                    throw new ArgumentException("Airport identifier cannot be null or empty", nameof(icaoIdOrIdent));
                }

                var metar = await _context.Metars
                    .FirstOrDefaultAsync(m => m.StationId == icaoIdOrIdent.ToUpper());

                if (metar == null)
                {
                    _logger.LogDebug("METAR not found directly, searching for airport: {IcaoIdOrIdent}", icaoIdOrIdent);

                    var airport = await _context.Airports
                        .FirstOrDefaultAsync(a => a.ArptId == icaoIdOrIdent.ToUpper() ||
                                                a.IcaoId == icaoIdOrIdent.ToUpper());

                    if (airport == null)
                    {
                        _logger.LogWarning("Airport not found for identifier: {IcaoIdOrIdent}", icaoIdOrIdent);
                        throw new ResourceNotFoundException($"Airport not found for ICAO code or identifier: {icaoIdOrIdent}");
                    }

                    var modifiedIdent = airport.StateCode switch
                    {
                        "AK" or "HI" => $"P{airport.ArptId}",
                        _ => $"K{airport.ArptId}"
                    };

                    _logger.LogDebug("Searching for METAR with modified identifier: {ModifiedIdent}", modifiedIdent);
                    metar = await _context.Metars
                        .FirstOrDefaultAsync(m => m.StationId == modifiedIdent);
                }

                if (metar == null)
                {
                    _logger.LogWarning("METAR not found for airport: {IcaoIdOrIdent}", icaoIdOrIdent);
                    throw new ResourceNotFoundException($"METAR not found for airport with ICAO ID: {icaoIdOrIdent}");
                }

                _logger.LogInformation("Successfully retrieved METAR for airport: {IcaoIdOrIdent}", icaoIdOrIdent);
                return MetarMapper.ToDto(metar);
            }
            catch (Exception ex) when (ex is not ResourceNotFoundException)
            {
                _logger.LogError(ex, "Error retrieving METAR for airport: {IcaoIdOrIdent}", icaoIdOrIdent);
                throw;
            }
        }

        public async Task<IEnumerable<MetarDto>> GetMetarsByState(string stateCode)
        {
            try
            {
                _logger.LogInformation("Retrieving METARs for state: {StateCode}", stateCode);

                if (string.IsNullOrWhiteSpace(stateCode))
                {
                    throw new ArgumentException("State code cannot be null or empty", nameof(stateCode));
                }

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

                _logger.LogInformation("Retrieved {Count} METARs for state: {StateCode}",
                    metars.Count, stateCode);

                return metars.Select(MetarMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving METARs for state: {StateCode}", stateCode);
                throw;
            }
        }

        public async Task<IEnumerable<MetarDto>> GetMetarsByStates(string[] stateCodes)
        {
            try
            {
                _logger.LogInformation("Retrieving METARs for states: {@StateCodes}", stateCodes);

                if (stateCodes == null || !stateCodes.Any())
                {
                    throw new ArgumentException("State codes array cannot be null or empty", nameof(stateCodes));
                }

                if (stateCodes.Any(string.IsNullOrWhiteSpace))
                {
                    throw new ArgumentException("State codes cannot contain null or empty values", nameof(stateCodes));
                }

                var upperStateCodes = stateCodes.Select(s => s?.ToUpper())
                    .Where(s => s != null)
                    .ToHashSet();

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

                _logger.LogInformation("Retrieved {Count} METARs for states: {@StateCodes}",
                    metars.Count, stateCodes);

                return metars.Select(MetarMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving METARs for states: {@StateCodes}", stateCodes);
                throw;
            }
        }
    }
}