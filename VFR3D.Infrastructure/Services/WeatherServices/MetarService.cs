using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Mappers;

namespace VFR3D.Infrastructure.Services.WeatherServices
{
    public class MetarService : IMetarService
    {
        private readonly VFR3DDbContext _context;
        private readonly ICensusGeocodingService _censusService;
        private readonly ILogger<MetarService> _logger;

        public MetarService(
            VFR3DDbContext context,
            ICensusGeocodingService censusService,
            ILogger<MetarService> logger)
        {
            _context = context;
            _censusService = censusService;
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
            var metars = await _context.Metars
                .Where(m => m.Latitude != null && m.Longitude != null)
                .ToListAsync();

            var stateMetars = new List<MetarDto>();

            foreach (var metar in metars)
            {
                try
                {
                    var stateInfo = await _censusService.GetStateFromCoordinates(
                        metar.Latitude!.Value,
                        metar.Longitude!.Value);

                    if (stateInfo?.StateCode == stateCode.ToUpper())
                    {
                        stateMetars.Add(MetarMapper.ToDto(metar));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Error checking state for METAR at coordinates: {Latitude}, {Longitude}",
                        metar.Latitude,
                        metar.Longitude);
                }
            }

            return stateMetars;
        }

        public async Task<IEnumerable<MetarDto>> GetMetarsByStates(string[] stateCodes)
        {
            var upperStateCodes = stateCodes.Select(s => s.ToUpper()).ToHashSet();
            var metars = await _context.Metars
                .Where(m => m.Latitude != null && m.Longitude != null)
                .ToListAsync();

            var stateMetars = new List<MetarDto>();

            foreach (var metar in metars)
            {
                try
                {
                    var stateInfo = await _censusService.GetStateFromCoordinates(
                        metar.Latitude!.Value,
                        metar.Longitude!.Value);

                    if (stateInfo != null && upperStateCodes.Contains(stateInfo.StateCode))
                    {
                        stateMetars.Add(MetarMapper.ToDto(metar));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Error checking state for METAR at coordinates: {Latitude}, {Longitude}",
                        metar.Latitude,
                        metar.Longitude);
                }
            }

            return stateMetars;
        }
    }
}
