using VFR3D.Infrastructure.Dtos;

namespace VFR3D.Infrastructure.Interfaces
{
    public interface ICensusGeocodingService
    {
        Task<StateInfoDto?> GetStateFromCoordinates(double latitude, double longitude);
    }
}
