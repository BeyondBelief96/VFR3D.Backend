using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.AirportInformationServices
{
    public class AirspaceService : IAirspaceService
    {
        private readonly VFR3DDbContext _context;
        private readonly ILogger<AirspaceService> _logger;

        public AirspaceService(
            VFR3DDbContext context,
            ILogger<AirspaceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AirspaceDto>> GetByClasses(string[] airspaceClasses)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by classes: {Classes}", 
                    string.Join(", ", airspaceClasses));

                var upperClasses = airspaceClasses.Select(c => c.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => a.Class != null && upperClasses.Contains(a.Class))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by classes: {Classes}", 
                    string.Join(", ", airspaceClasses));
                throw;
            }
        }

        public async Task<IEnumerable<AirspaceDto>> GetByCities(string[] cities)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by cities: {Cities}", 
                    string.Join(", ", cities));

                var upperCities = cities.Select(c => c.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => a.City != null && upperCities.Contains(a.City.ToUpper()))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by cities: {Cities}", 
                    string.Join(", ", cities));
                throw;
            }
        }

        public async Task<IEnumerable<AirspaceDto>> GetByStates(string[] states)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by states: {States}", 
                    string.Join(", ", states));

                var upperStates = states.Select(s => s.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => a.State != null && upperStates.Contains(a.State))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by states: {States}", 
                    string.Join(", ", states));
                throw;
            }
        }

        public async Task<IEnumerable<SpecialUseAirspaceDto>> GetByTypeCodes(string[] typeCodes)
        {
            try
            {
                _logger.LogInformation("Getting special use airspaces by type codes: {TypeCodes}", 
                    string.Join(", ", typeCodes));

                var upperTypeCodes = typeCodes.Select(t => t.ToUpper()).ToArray();
                var airspaces = await _context.SpecialUseAirspaces
                    .Where(a => a.TypeCode != null && upperTypeCodes.Contains(a.TypeCode))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting special use airspaces by type codes: {TypeCodes}", 
                    string.Join(", ", typeCodes));
                throw;
            }
        }

        public async Task<IEnumerable<AirspaceDto>> GetByIcaoOrIdents(string[] icaoOrIdents)
        {
            try
            {
                _logger.LogInformation("Getting airspaces by ICAO codes or idents: {IcaoOrIdents}", 
                    string.Join(", ", icaoOrIdents));

                var upperCodes = icaoOrIdents.Select(i => i.ToUpper()).ToArray();
                var airspaces = await _context.Airspaces
                    .Where(a => 
                        (a.IcaoId != null && upperCodes.Contains(a.IcaoId)) || 
                        (a.Ident != null && upperCodes.Contains(a.Ident)))
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                return airspaces.Select(AirspaceMapper.ToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airspaces by ICAO codes or idents: {IcaoOrIdents}", 
                    string.Join(", ", icaoOrIdents));
                throw;
            }
        }
    }
}