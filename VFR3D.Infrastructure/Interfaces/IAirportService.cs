using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface IAirportService
{
    Task<IEnumerable<AirportDto>> GetAllAirports(string? search = null);
    Task<AirportDto> GetAirportByIcaoCodeOrIdent(string icaoCodeOrIdent);
    Task<IEnumerable<AirportDto>> GetAirportsByState(string stateCode);
    Task<IEnumerable<AirportDto>> GetAirportsByStates(string[] stateCodes);
    Task<IEnumerable<AirportDto>> GetAirportsByIcaoCodesOrIdents(string[] codesOrIdents);
    Task<IEnumerable<AirportDto>> GetAirportsByPrefix(string prefix);
}