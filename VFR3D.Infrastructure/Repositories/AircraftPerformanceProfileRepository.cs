using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Repositories
{
    public class AircraftPerformanceProfileRepository : IAircraftPerformanceProfileRepository
    {
        private readonly VFR3DDbContext _context;

        public AircraftPerformanceProfileRepository(VFR3DDbContext context)
        {
            _context = context;
        }

        public async Task<AircraftPerformanceProfile?> GetByIdAsync(string id)
        {
            return await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<AircraftPerformanceProfile>> GetByUserIdAsync(string userId)
        {
            return await _context.AircraftPerformanceProfiles
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<AircraftPerformanceProfile> CreateAsync(AircraftPerformanceProfile profile)
        {
            _context.AircraftPerformanceProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<AircraftPerformanceProfile> UpdateAsync(AircraftPerformanceProfile profile)
        {
            _context.AircraftPerformanceProfiles.Update(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task DeleteAsync(string id)
        {
            var profile = await GetByIdAsync(id);
            if (profile != null)
            {
                _context.AircraftPerformanceProfiles.Remove(profile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByNameAndUserIdAsync(string profileName, string userId)
        {
            return await _context.AircraftPerformanceProfiles
                .AnyAsync(p => p.ProfileName == profileName && p.UserId == userId);
        }

        public async Task<bool> IsUsedByFlightsAsync(string profileId)
        {
            return await _context.Flights
                .AnyAsync(f => f.AircraftPerformanceId == profileId);
        }

        public async Task<AircraftPerformanceProfile?> GetByIdAndUserIdAsync(string id, string userId)
        {
            return await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }
    }
}
