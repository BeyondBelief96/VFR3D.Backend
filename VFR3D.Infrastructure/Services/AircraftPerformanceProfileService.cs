using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.AircraftPerformanceProfiles;
using VFR3D.Infrastructure.Dtos.Mappers;
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

            // Get aircraft unit preferences if profile is linked to an aircraft
            var (airspeedUnits, lengthUnits) = await GetAircraftUnits(request.AircraftId);

            // Convert input values from user units to canonical units (knots/feet)
            var profile = AircraftPerformanceProfileMapper.CreateFromRequest(
                request.UserId,
                request,
                airspeedUnits,
                lengthUnits);

            _context.AircraftPerformanceProfiles.Add(profile);
            await _context.SaveChangesAsync();

            // Return DTO in user units
            return AircraftPerformanceProfileMapper.MapToDto(profile, airspeedUnits, lengthUnits);
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

            // Get aircraft unit preferences if profile is linked to an aircraft
            var (airspeedUnits, lengthUnits) = await GetAircraftUnits(request.AircraftId);

            // Update profile properties with conversion from user units to canonical units
            AircraftPerformanceProfileMapper.UpdateFromRequest(profile, request, airspeedUnits, lengthUnits);

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

            // Return DTO in user units
            return AircraftPerformanceProfileMapper.MapToDto(profile, airspeedUnits, lengthUnits);
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

            // Fetch all aircraft for this user to get their unit preferences
            var aircraftIds = profiles
                .Where(p => p.AircraftId != null)
                .Select(p => p.AircraftId!)
                .Distinct()
                .ToList();

            var aircraftDict = await _context.Aircraft
                .Where(a => aircraftIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => (a.AirspeedUnits, a.LengthUnits));

            return profiles.Select(p =>
            {
                if (p.AircraftId != null && aircraftDict.TryGetValue(p.AircraftId, out var units))
                {
                    return AircraftPerformanceProfileMapper.MapToDto(p, units.AirspeedUnits, units.LengthUnits);
                }
                // No linked aircraft - return in canonical units
                return AircraftPerformanceProfileMapper.MapToDto(p);
            }).ToList();
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

    /// <summary>
    /// Gets the unit preferences for an aircraft, or returns canonical units if no aircraft is specified.
    /// </summary>
    private async Task<(AirspeedUnits AirspeedUnits, LengthUnits LengthUnits)> GetAircraftUnits(string? aircraftId)
    {
        if (string.IsNullOrEmpty(aircraftId))
        {
            return (AirspeedUnits.Knots, LengthUnits.Feet);
        }

        var aircraft = await _context.Aircraft.FindAsync(aircraftId);
        if (aircraft == null)
        {
            return (AirspeedUnits.Knots, LengthUnits.Feet);
        }

        return (aircraft.AirspeedUnits, aircraft.LengthUnits);
    }
}