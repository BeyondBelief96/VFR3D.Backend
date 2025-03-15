using Amazon.SecretsManager.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services
{
    public class AirportService : IAirportService
    {
        private readonly VFR3DDbContext _context;
        private readonly ILogger<AirportService> _logger;

        public AirportService(
            VFR3DDbContext context,
            ILogger<AirportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AirportDto>> GetAllAirports(string? search = null)
        {
            try
            {
                _logger.LogInformation("Getting all airports with search: {Search}", search);

                var query = _context.Airports.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToUpper();
                    query = query.Where(a => 
                        (a.IcaoId != null && a.IcaoId.ToUpper().Contains(search)) ||
                        (a.ArptId != null && a.ArptId.ToUpper().Contains(search))
                    );
                }

                var airports = await query.ToListAsync();
                return airports.Select(AirportMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all airports with search: {Search}", search);
                throw;
            }
        }

        public async Task<AirportDto> GetAirportByIcaoCodeOrIdent(string icaoCodeOrIdent)
        {
            try
            {
                _logger.LogInformation("Getting airport by ICAO code or ident: {IcaoCodeOrIdent}", icaoCodeOrIdent);

                var airport = await _context.Airports
                    .FirstOrDefaultAsync(a => 
                        a.IcaoId == icaoCodeOrIdent.ToUpper() || 
                        a.ArptId == icaoCodeOrIdent.ToUpper());

                if (airport == null)
                {
                    throw new ResourceNotFoundException(
                        $"Airport not found for ICAO code or identifier: {icaoCodeOrIdent}");
                }

                return AirportMapper.ToDto(airport);
            }
            catch (Exception ex) when (ex is not ResourceNotFoundException)
            {
                _logger.LogError(ex, "Error getting airport by ICAO code or ident: {IcaoCodeOrIdent}", icaoCodeOrIdent);
                throw;
            }
        }

        public async Task<IEnumerable<AirportDto>> GetAirportsByState(string stateCode)
        {
            try
            {
                _logger.LogInformation("Getting airports by state: {StateCode}", stateCode);

                var airports = await _context.Airports
                    .Where(a => a.StateCode == stateCode.ToUpper())
                    .ToListAsync();

                return airports.Select(AirportMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airports by state: {StateCode}", stateCode);
                throw;
            }
        }

        public async Task<IEnumerable<AirportDto>> GetAirportsByStates(string[] stateCodes)
        {
            try
            {
                _logger.LogInformation("Getting airports by states: {StateCodes}", string.Join(", ", stateCodes));

                var upperStateCodes = stateCodes.Select(s => s.ToUpper()).ToArray();
                var airports = await _context.Airports
                    .Where(a => a.StateCode != null && upperStateCodes.Contains(a.StateCode))
                    .ToListAsync();

                return airports.Select(AirportMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airports by states: {StateCodes}", string.Join(", ", stateCodes));
                throw;
            }
        }

        public async Task<IEnumerable<AirportDto>> GetAirportsByIcaoCodesOrIdents(string[] codesOrIdents)
        {
            try
            {
                _logger.LogInformation("Getting airports by ICAO codes or idents: {CodesOrIdents}", 
                    string.Join(", ", codesOrIdents));

                var upperCodes = codesOrIdents.Select(c => c.ToUpper()).ToArray();
                var airports = await _context.Airports
                    .Where(a => 
                        (a.IcaoId != null && upperCodes.Contains(a.IcaoId)) || 
                        (a.ArptId != null && upperCodes.Contains(a.ArptId)))
                    .ToListAsync();

                return airports.Select(AirportMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airports by ICAO codes or idents: {CodesOrIdents}", 
                    string.Join(", ", codesOrIdents));
                throw;
            }
        }

        public async Task<IEnumerable<AirportDto>> GetAirportsByPrefix(string prefix)
        {
            try
            {
                _logger.LogInformation("Getting airports by prefix: {Prefix}", prefix);

                if (string.IsNullOrWhiteSpace(prefix))
                {
                    return Enumerable.Empty<AirportDto>();
                }

                var upperPrefix = prefix.ToUpper();
                var airports = await _context.Airports
                    .Where(a => 
                        (a.IcaoId != null && a.IcaoId.ToUpper().StartsWith(upperPrefix)) || 
                        (a.ArptId != null && a.ArptId.ToUpper().StartsWith(upperPrefix)))
                    .ToListAsync();

                return airports.Select(AirportMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airports by prefix: {Prefix}", prefix);
                throw;
            }
        }
    }
}