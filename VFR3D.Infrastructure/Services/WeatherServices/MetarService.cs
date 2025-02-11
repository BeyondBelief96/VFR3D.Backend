using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.WeatherServices
{
    internal class MetarService : IMetarService
    {
        public Task<MetarDto> GetMetarForAirport(string icaoIdOrIdent)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MetarDto>> GetMetarsByState(string stateCode)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MetarDto>> GetMetarsByStates(string[] stateCodes)
        {
            throw new NotImplementedException();
        }
    }
}
