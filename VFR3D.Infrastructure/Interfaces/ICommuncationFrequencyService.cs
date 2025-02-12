using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces;

public interface ICommunicationFrequencyService
{
    Task<IEnumerable<CommunicationFrequencyDto>> GetFrequenciesByServicedFacility(string servicedFacility);
}