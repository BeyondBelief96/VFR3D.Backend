using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Exceptions;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Aircraft;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class AircraftService : IAircraftService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<AircraftService> _logger;

    public AircraftService(VFR3DDbContext context, ILogger<AircraftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AircraftDto> CreateAircraft(string userId, CreateAircraftRequestDto request)
    {
        try
        {
            // Check for duplicate tail number for this user
            var existingAircraft = await _context.Aircraft
                .AnyAsync(a => a.UserId == userId && a.TailNumber == request.TailNumber);

            if (existingAircraft)
            {
                _logger.LogWarning("Aircraft with tail number {TailNumber} already exists for user {UserId}",
                    request.TailNumber, userId);
                throw new DuplicateTailNumberException(request.TailNumber);
            }

            var aircraft = AircraftMapper.CreateFromRequest(userId, request);

            _context.Aircraft.Add(aircraft);
            await _context.SaveChangesAsync();

            return AircraftMapper.MapToDto(aircraft);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating aircraft for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AircraftDto> UpdateAircraft(string userId, string id, UpdateAircraftRequestDto request)
    {
        try
        {
            var aircraft = await _context.Aircraft
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (aircraft == null)
            {
                throw new AircraftNotFoundException(userId, id);
            }

            // Check for duplicate tail number if it changed
            if (aircraft.TailNumber != request.TailNumber)
            {
                var duplicateExists = await _context.Aircraft
                    .AnyAsync(a => a.UserId == userId && a.TailNumber == request.TailNumber && a.Id != id);

                if (duplicateExists)
                {
                    _logger.LogWarning("Aircraft with tail number {TailNumber} already exists for user {UserId}",
                        request.TailNumber, userId);
                    throw new DuplicateTailNumberException(request.TailNumber);
                }
            }

            AircraftMapper.UpdateFromRequest(aircraft, request);
            await _context.SaveChangesAsync();

            return AircraftMapper.MapToDto(aircraft);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating aircraft {AircraftId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<AircraftDto?> GetAircraft(string userId, string id)
    {
        try
        {
            var aircraft = await _context.Aircraft
                .Include(a => a.PerformanceProfiles)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            return aircraft == null ? null : AircraftMapper.MapToDto(aircraft, includeProfiles: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aircraft {AircraftId} for user {UserId}", id, userId);
            throw;
        }
    }

    public async Task<List<AircraftDto>> GetAircraftByUserId(string userId)
    {
        try
        {
            var aircraft = await _context.Aircraft
                .Include(a => a.PerformanceProfiles)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return aircraft.Select(a => AircraftMapper.MapToDto(a, includeProfiles: true)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aircraft for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteAircraft(string userId, string id)
    {
        try
        {
            var aircraft = await _context.Aircraft
                .Include(a => a.PerformanceProfiles)
                .Include(a => a.Flights)
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (aircraft == null)
            {
                throw new AircraftNotFoundException(userId, id);
            }

            // Check if aircraft has flights directly linked to it
            if (aircraft.Flights.Any())
            {
                throw new ResourceInUseException("Aircraft", id, "it is being used by existing flights");
            }

            // Check if any of the aircraft's performance profiles have flights linked to them
            var profileIds = aircraft.PerformanceProfiles.Select(p => p.Id).ToList();
            if (profileIds.Any())
            {
                var hasFlightsUsingProfiles = await _context.Flights
                    .AnyAsync(f => profileIds.Contains(f.AircraftPerformanceId));

                if (hasFlightsUsingProfiles)
                {
                    throw new ResourceInUseException("Aircraft", id, "its performance profiles are being used by existing flights");
                }
            }

            // Performance profiles will be cascade deleted
            _context.Aircraft.Remove(aircraft);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting aircraft {AircraftId} for user {UserId}", id, userId);
            throw;
        }
    }
}
