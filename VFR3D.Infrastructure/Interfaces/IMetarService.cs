using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces
{
    public interface IMetarService
    {
        Task<MetarDto> GetMetarForAirport(string icaoIdOrIdent);
        Task<IEnumerable<MetarDto>> GetMetarsByState(string stateCode);
        Task<IEnumerable<MetarDto>> GetMetarsByStates(string[] stateCodes);
    }
}
