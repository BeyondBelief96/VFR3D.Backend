using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class AircraftPerformanceProfileService : IAircraftPerformanceProfileService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<AircraftPerformanceProfileService> _logger;
    private readonly IFlightService _flightService;

    public AircraftPerformanceProfileService(
        VFR3DDbContext context,
        IFlightService flightService,
        ILogger<AircraftPerformanceProfileService> logger)
    {
        _context = context;
        _flightService = flightService;
        _logger = logger;
    }

    public async Task<AircraftPerformanceProfileDto> SaveProfile(SaveAircraftPerformanceProfileRequestDto request)
    {
        try
        {
            // Check if a performance profile for this user already exists with the given name. 
            var existingProfile = _context.AircraftPerformanceProfiles.Any(p => p.ProfileName == request.ProfileName && p.UserId == request.UserId);
            if (existingProfile)
            {
                _logger.LogWarning($"Profile with name {request.ProfileName} already exists");
                throw new DuplicateNameException($"Profile with name {request.ProfileName} already exists");
            }
            
            var profile = new AircraftPerformanceProfile
            {
                Id = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                AircraftId = request.AircraftId,
                ProfileName = request.ProfileName,
                ClimbTrueAirspeed = request.ClimbTrueAirspeed,
                CruiseTrueAirspeed = request.CruiseTrueAirspeed,
                CruiseFuelBurn = request.CruiseFuelBurn,
                ClimbFuelBurn = request.ClimbFuelBurn,
                DescentFuelBurn = request.DescentFuelBurn,
                ClimbFpm = request.ClimbFpm,
                DescentFpm = request.DescentFpm,
                DescentTrueAirspeed = request.DescentTrueAirspeed,
                SttFuelGals = request.SttFuelGals,
                FuelOnBoardGals = request.FuelOnBoardGals
            };

            _context.AircraftPerformanceProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return MapToDto(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving aircraft performance profile for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<AircraftPerformanceProfileDto> UpdateProfile(string id, UpdateAircraftPerformanceProfileRequestDto request)
    {
        try
        {
            var profile = await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == request.UserId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profile not found with ID {id}");
            }

            // Update profile properties
            profile.AircraftId = request.AircraftId;
            profile.ProfileName = request.ProfileName;
            profile.ClimbTrueAirspeed = request.ClimbTrueAirspeed;
            profile.CruiseTrueAirspeed = request.CruiseTrueAirspeed;
            profile.CruiseFuelBurn = request.CruiseFuelBurn;
            profile.ClimbFuelBurn = request.ClimbFuelBurn;
            profile.DescentFuelBurn = request.DescentFuelBurn;
            profile.ClimbFpm = request.ClimbFpm;
            profile.DescentFpm = request.DescentFpm;
            profile.DescentTrueAirspeed = request.DescentTrueAirspeed;
            profile.SttFuelGals = request.SttFuelGals;
            profile.FuelOnBoardGals = request.FuelOnBoardGals;

            // Save profile changes
            await _context.SaveChangesAsync();

            // Retrieve all flights using the updated profile
            var flights = await _context.Flights
                .Where(f => f.AircraftPerformanceId == id)
                .ToListAsync();

            // Update flights with the new profile data and recalculate navigation logs
            foreach (var flight in flights)
            {
                // Update flight with new profile data
                flight.AircraftPerformanceProfile = profile;
                await _context.SaveChangesAsync();
                await _flightService.RegenerateNavlog(request.UserId, flight.Id);
            }

            // Save flight changes
            await _context.SaveChangesAsync();

            return MapToDto(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating aircraft performance profile {ProfileId} for user {UserId}",
                id, request.UserId);
            throw;
        }
    }

    public async Task<List<AircraftPerformanceProfileDto>> GetProfilesByUserId(string userId)
    {
        try
        {
            var profiles = await _context.AircraftPerformanceProfiles
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return profiles.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aircraft performance profiles for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteProfile(string userId, string profileId)
    {
        try
        {
            // First check if the profile is being used by any flights
            var hasFlights = await _context.Flights
                .AnyAsync(f => f.AircraftPerformanceId == profileId);

            if (hasFlights)
            {
                throw new InvalidOperationException("Cannot delete profile as it is being used by existing flights");
            }

            var profile = await _context.AircraftPerformanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"Profile not found with ID {profileId}");
            }

            _context.AircraftPerformanceProfiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting aircraft performance profile {ProfileId} for user {UserId}", 
                profileId, userId);
            throw;
        }
    }

    private static AircraftPerformanceProfileDto MapToDto(AircraftPerformanceProfile profile)
    {
        return new AircraftPerformanceProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            AircraftId = profile.AircraftId,
            ProfileName = profile.ProfileName,
            ClimbTrueAirspeed = profile.ClimbTrueAirspeed,
            CruiseTrueAirspeed = profile.CruiseTrueAirspeed,
            CruiseFuelBurn = profile.CruiseFuelBurn,
            ClimbFuelBurn = profile.ClimbFuelBurn,
            DescentFuelBurn = profile.DescentFuelBurn,
            ClimbFpm = profile.ClimbFpm,
            DescentFpm = profile.DescentFpm,
            DescentTrueAirspeed = profile.DescentTrueAirspeed,
            SttFuelGals = profile.SttFuelGals,
            FuelOnBoardGals = profile.FuelOnBoardGals
        };
    }
}