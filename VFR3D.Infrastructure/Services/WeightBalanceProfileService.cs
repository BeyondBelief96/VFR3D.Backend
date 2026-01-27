using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.WeightBalance;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.WeightBalance;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services;

public class WeightBalanceProfileService : IWeightBalanceProfileService
{
    private readonly VFR3DDbContext _context;
    private readonly ILogger<WeightBalanceProfileService> _logger;

    public WeightBalanceProfileService(VFR3DDbContext context, ILogger<WeightBalanceProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WeightBalanceProfileDto> CreateProfile(string userId, CreateWeightBalanceProfileRequestDto request)
    {
        try
        {
            // Check for duplicate profile name for this user
            var existingProfile = await _context.WeightBalanceProfiles
                .AnyAsync(p => p.UserId == userId && p.ProfileName == request.ProfileName);

            if (existingProfile)
            {
                _logger.LogWarning("W&B profile with name {ProfileName} already exists for user {UserId}",
                    request.ProfileName, userId);
                throw new DuplicateNameException($"Weight & Balance profile with name '{request.ProfileName}' already exists");
            }

            // Verify aircraft exists if provided
            if (!string.IsNullOrEmpty(request.AircraftId))
            {
                var aircraftExists = await _context.Aircraft
                    .AnyAsync(a => a.Id == request.AircraftId && a.UserId == userId);

                if (!aircraftExists)
                {
                    throw new KeyNotFoundException($"Aircraft not found with ID {request.AircraftId}");
                }
            }

            var profile = WeightBalanceProfileMapper.CreateFromRequest(userId, request);

            _context.WeightBalanceProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return WeightBalanceProfileMapper.MapToDto(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating W&B profile for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WeightBalanceProfileDto?> GetProfile(string userId, string profileId)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            return profile == null ? null : WeightBalanceProfileMapper.MapToDto(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting W&B profile {ProfileId} for user {UserId}", profileId, userId);
            throw;
        }
    }

    public async Task<List<WeightBalanceProfileDto>> GetProfilesByUser(string userId)
    {
        try
        {
            var profiles = await _context.WeightBalanceProfiles
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return profiles.Select(WeightBalanceProfileMapper.MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting W&B profiles for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<WeightBalanceProfileDto>> GetProfilesByAircraft(string userId, string aircraftId)
    {
        try
        {
            var profiles = await _context.WeightBalanceProfiles
                .Where(p => p.UserId == userId && p.AircraftId == aircraftId)
                .ToListAsync();

            return profiles.Select(WeightBalanceProfileMapper.MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting W&B profiles for aircraft {AircraftId} user {UserId}", aircraftId, userId);
            throw;
        }
    }

    public async Task<WeightBalanceProfileDto> UpdateProfile(string userId, string profileId, UpdateWeightBalanceProfileRequestDto request)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"W&B profile not found with ID {profileId}");
            }

            // Check for duplicate profile name if it changed
            if (profile.ProfileName != request.ProfileName)
            {
                var duplicateExists = await _context.WeightBalanceProfiles
                    .AnyAsync(p => p.UserId == userId && p.ProfileName == request.ProfileName && p.Id != profileId);

                if (duplicateExists)
                {
                    _logger.LogWarning("W&B profile with name {ProfileName} already exists for user {UserId}",
                        request.ProfileName, userId);
                    throw new DuplicateNameException($"Weight & Balance profile with name '{request.ProfileName}' already exists");
                }
            }

            // Verify aircraft exists if provided
            if (!string.IsNullOrEmpty(request.AircraftId))
            {
                var aircraftExists = await _context.Aircraft
                    .AnyAsync(a => a.Id == request.AircraftId && a.UserId == userId);

                if (!aircraftExists)
                {
                    throw new KeyNotFoundException($"Aircraft not found with ID {request.AircraftId}");
                }
            }

            WeightBalanceProfileMapper.UpdateFromRequest(profile, request);
            await _context.SaveChangesAsync();

            return WeightBalanceProfileMapper.MapToDto(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating W&B profile {ProfileId} for user {UserId}", profileId, userId);
            throw;
        }
    }

    public async Task DeleteProfile(string userId, string profileId)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"W&B profile not found with ID {profileId}");
            }

            _context.WeightBalanceProfiles.Remove(profile);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting W&B profile {ProfileId} for user {UserId}", profileId, userId);
            throw;
        }
    }

    public async Task<WeightBalanceCalculationResultDto> Calculate(string userId, string profileId, WeightBalanceCalculationRequestDto request)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new KeyNotFoundException($"W&B profile not found with ID {profileId}");
            }

            // Select envelope (use specified or first)
            var envelope = string.IsNullOrEmpty(request.EnvelopeId)
                ? profile.CgEnvelopes.FirstOrDefault()
                : profile.CgEnvelopes.FirstOrDefault(e => e.Id == request.EnvelopeId);

            if (envelope == null)
            {
                throw new InvalidOperationException("No CG envelope found for calculation");
            }

            var warnings = new List<string>();
            var stationBreakdown = new List<StationBreakdownDto>();

            // Start with empty weight
            double totalWeight = profile.EmptyWeight;
            double totalMoment = profile.EmptyWeight * profile.EmptyWeightArm;

            stationBreakdown.Add(new StationBreakdownDto
            {
                StationId = "empty",
                Name = "Empty Weight",
                Weight = profile.EmptyWeight,
                Arm = profile.EmptyWeightArm,
                Moment = profile.EmptyWeight * profile.EmptyWeightArm
            });

            // Track fuel station for landing calculation
            LoadingStation? fuelStation = null;
            double fuelWeight = 0;

            // Process each loaded station
            foreach (var load in request.LoadedStations)
            {
                var station = profile.LoadingStations.FirstOrDefault(s => s.Id == load.StationId);
                if (station == null)
                {
                    warnings.Add($"Unknown station ID: {load.StationId}");
                    continue;
                }

                double stationWeight;
                string stationName;

                if (station.IsFuelStation && load.FuelGallons.HasValue)
                {
                    fuelStation = station;
                    var weightPerGallon = station.FuelWeightPerGallon ?? 6.0;
                    stationWeight = load.FuelGallons.Value * weightPerGallon;
                    fuelWeight = stationWeight;
                    stationName = $"{station.Name} ({load.FuelGallons.Value:F1} gal)";

                    // Check fuel capacity
                    if (station.FuelCapacityGallons.HasValue && load.FuelGallons.Value > station.FuelCapacityGallons.Value)
                    {
                        warnings.Add($"{station.Name}: Fuel exceeds capacity ({load.FuelGallons.Value:F1} > {station.FuelCapacityGallons.Value:F1} gal)");
                    }
                }
                else if (load.Weight.HasValue)
                {
                    stationWeight = load.Weight.Value;
                    stationName = station.Name;

                    // Check max weight
                    if (stationWeight > station.MaxWeight)
                    {
                        warnings.Add($"{station.Name}: Weight exceeds maximum ({stationWeight:F1} > {station.MaxWeight:F1})");
                    }
                }
                else
                {
                    continue;
                }

                totalWeight += stationWeight;
                totalMoment += stationWeight * station.Arm;

                stationBreakdown.Add(new StationBreakdownDto
                {
                    StationId = station.Id,
                    Name = stationName,
                    Weight = stationWeight,
                    Arm = station.Arm,
                    Moment = stationWeight * station.Arm
                });
            }

            // Calculate takeoff CG
            double takeoffCgArm = totalWeight > 0 ? totalMoment / totalWeight : 0;
            bool takeoffWithinEnvelope = IsPointInEnvelope(totalWeight, takeoffCgArm, envelope.Limits);

            // Check takeoff weight limits
            if (totalWeight > profile.MaxTakeoffWeight)
            {
                warnings.Add($"Takeoff weight ({totalWeight:F1}) exceeds max takeoff weight ({profile.MaxTakeoffWeight:F1})");
            }

            if (profile.MaxRampWeight.HasValue && totalWeight > profile.MaxRampWeight.Value)
            {
                warnings.Add($"Ramp weight ({totalWeight:F1}) exceeds max ramp weight ({profile.MaxRampWeight.Value:F1})");
            }

            if (!takeoffWithinEnvelope)
            {
                warnings.Add("Takeoff CG is outside the envelope limits");
            }

            var takeoffResult = new WeightBalanceCgResultDto
            {
                TotalWeight = Math.Round(totalWeight, 1),
                TotalMoment = Math.Round(totalMoment, 1),
                CgArm = Math.Round(takeoffCgArm, 2),
                IsWithinEnvelope = takeoffWithinEnvelope
            };

            // Calculate landing CG if fuel burn provided
            WeightBalanceCgResultDto? landingResult = null;

            if (request.FuelBurnGallons.HasValue && fuelStation != null)
            {
                var weightPerGallon = fuelStation.FuelWeightPerGallon ?? 6.0;
                var fuelBurnWeight = request.FuelBurnGallons.Value * weightPerGallon;

                double landingWeight = totalWeight - fuelBurnWeight;
                double landingMoment = totalMoment - (fuelBurnWeight * fuelStation.Arm);
                double landingCgArm = landingWeight > 0 ? landingMoment / landingWeight : 0;
                bool landingWithinEnvelope = IsPointInEnvelope(landingWeight, landingCgArm, envelope.Limits);

                // Check landing weight limits
                var maxLandingWeight = profile.MaxLandingWeight ?? profile.MaxTakeoffWeight;
                if (landingWeight > maxLandingWeight)
                {
                    warnings.Add($"Landing weight ({landingWeight:F1}) exceeds max landing weight ({maxLandingWeight:F1})");
                }

                if (!landingWithinEnvelope)
                {
                    warnings.Add("Landing CG is outside the envelope limits");
                }

                landingResult = new WeightBalanceCgResultDto
                {
                    TotalWeight = Math.Round(landingWeight, 1),
                    TotalMoment = Math.Round(landingMoment, 1),
                    CgArm = Math.Round(landingCgArm, 2),
                    IsWithinEnvelope = landingWithinEnvelope
                };
            }

            return new WeightBalanceCalculationResultDto
            {
                Takeoff = takeoffResult,
                Landing = landingResult,
                StationBreakdown = stationBreakdown,
                EnvelopeName = envelope.Name,
                EnvelopeLimits = envelope.Limits.Select(WeightBalanceProfileMapper.MapEnvelopePointToDto).ToList(),
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating W&B for profile {ProfileId} user {UserId}", profileId, userId);
            throw;
        }
    }

    private static bool IsPointInEnvelope(double weight, double cgArm, List<CgEnvelopePoint> envelope)
    {
        if (envelope.Count < 3)
            return false;

        // Ray casting algorithm for point-in-polygon
        int n = envelope.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var pi = envelope[i];
            var pj = envelope[j];

            if ((pi.Weight > weight) != (pj.Weight > weight) &&
                cgArm < (pj.Arm - pi.Arm) * (weight - pi.Weight) / (pj.Weight - pi.Weight) + pi.Arm)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
