using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Interfaces
{
    public interface IAircraftPerformanceProfileRepository
    {
        Task<AircraftPerformanceProfile?> GetByIdAsync(string id);
        Task<List<AircraftPerformanceProfile>> GetByUserIdAsync(string userId);
        Task<AircraftPerformanceProfile> CreateAsync(AircraftPerformanceProfile profile);
        Task<AircraftPerformanceProfile> UpdateAsync(AircraftPerformanceProfile profile);
        Task DeleteAsync(string id);

        Task<bool> ExistsByNameAndUserIdAsync(string profileName, string userId);
        Task<bool> IsUsedByFlightsAsync(string profileId);
        Task<AircraftPerformanceProfile?> GetByIdAndUserIdAsync(string id, string userId);
    }
}
