using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Enums;
using VFR3D.Domain.Exceptions;
using VFR3D.Domain.ValueObjects.WeightBalance;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.WeightBalance;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Mappers;

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
                throw new DuplicateProfileNameException("WeightBalanceProfile", request.ProfileName);
            }

            // Verify aircraft exists if provided
            if (!string.IsNullOrEmpty(request.AircraftId))
            {
                var aircraftExists = await _context.Aircraft
                    .AnyAsync(a => a.Id == request.AircraftId && a.UserId == userId);

                if (!aircraftExists)
                {
                    throw new AircraftNotFoundException(request.AircraftId);
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

    public async Task<WeightBalanceProfileDto?> GetProfile(string userId, Guid profileId)
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

    public async Task<WeightBalanceProfileDto> UpdateProfile(string userId, Guid profileId, UpdateWeightBalanceProfileRequestDto request)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new WeightBalanceProfileNotFoundException(profileId.ToString());
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
                    throw new DuplicateProfileNameException("WeightBalanceProfile", request.ProfileName);
                }
            }

            // Verify aircraft exists if provided
            if (!string.IsNullOrEmpty(request.AircraftId))
            {
                var aircraftExists = await _context.Aircraft
                    .AnyAsync(a => a.Id == request.AircraftId && a.UserId == userId);

                if (!aircraftExists)
                {
                    throw new AircraftNotFoundException(request.AircraftId);
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

    public async Task DeleteProfile(string userId, Guid profileId)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new WeightBalanceProfileNotFoundException(profileId.ToString());
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

    public async Task<WeightBalanceCalculationResultDto> Calculate(string userId, Guid profileId, WeightBalanceCalculationRequestDto request)
    {
        try
        {
            var profile = await _context.WeightBalanceProfiles
                .FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);

            if (profile == null)
            {
                throw new WeightBalanceProfileNotFoundException(profileId.ToString());
            }

            // Select envelope (use specified or first)
            var envelope = string.IsNullOrEmpty(request.EnvelopeId)
                ? profile.CgEnvelopes.FirstOrDefault()
                : profile.CgEnvelopes.FirstOrDefault(e => e.Id == request.EnvelopeId);

            if (envelope == null)
            {
                throw new ValidationException("EnvelopeId", "No CG envelope found for calculation");
            }

            var warnings = new List<string>();
            var stationBreakdown = new List<StationBreakdownDto>();

            // Start with empty weight (empty weight always uses arm directly, not loading graph)
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

                switch (station.StationType)
                {
                    case LoadingStationType.Fuel when load.FuelGallons.HasValue:
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
                        break;

                    case LoadingStationType.Oil when load.OilQuarts.HasValue:
                        var weightPerQuart = station.OilWeightPerQuart ?? 1.875; // Default aviation oil weight
                        stationWeight = load.OilQuarts.Value * weightPerQuart;
                        stationName = $"{station.Name} ({load.OilQuarts.Value:F1} qt)";

                        // Check oil capacity
                        if (station.OilCapacityQuarts.HasValue && load.OilQuarts.Value > station.OilCapacityQuarts.Value)
                        {
                            warnings.Add($"{station.Name}: Oil exceeds capacity ({load.OilQuarts.Value:F1} > {station.OilCapacityQuarts.Value:F1} qt)");
                        }
                        break;

                    case LoadingStationType.Standard when load.Weight.HasValue:
                        stationWeight = load.Weight.Value;
                        stationName = station.Name;

                        // Check max weight
                        if (stationWeight > station.MaxWeight)
                        {
                            warnings.Add($"{station.Name}: Weight exceeds maximum ({stationWeight:F1} > {station.MaxWeight:F1})");
                        }
                        break;

                    default:
                        // Allow weight override for fuel/oil stations if needed
                        if (load.Weight.HasValue)
                        {
                            stationWeight = load.Weight.Value;
                            stationName = station.Name;

                            if (stationWeight > station.MaxWeight)
                            {
                                warnings.Add($"{station.Name}: Weight exceeds maximum ({stationWeight:F1} > {station.MaxWeight:F1})");
                            }
                        }
                        else
                        {
                            continue;
                        }
                        break;
                }

                // Calculate moment using loading graph interpolation
                var (stationMoment, stationArm) = CalculateStationMoment(station, stationWeight, profile.LoadingGraphFormat);

                totalWeight += stationWeight;
                totalMoment += stationMoment;

                stationBreakdown.Add(new StationBreakdownDto
                {
                    StationId = station.Id,
                    Name = stationName,
                    Weight = stationWeight,
                    Arm = Math.Round(stationArm, 2),
                    Moment = Math.Round(stationMoment, 1)
                });
            }

            // Calculate takeoff CG
            double takeoffCgArm = totalWeight > 0 ? totalMoment / totalWeight : 0;

            // Determine the horizontal value for envelope check based on envelope format
            double takeoffHorizontalValue = envelope.Format == CgEnvelopeFormat.MomentDividedBy1000
                ? totalMoment / 1000.0
                : takeoffCgArm;

            bool takeoffWithinEnvelope = IsPointInEnvelope(totalWeight, takeoffHorizontalValue, envelope.Limits);

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
                var envelopeUnit = envelope.Format == CgEnvelopeFormat.MomentDividedBy1000 ? "Moment/1000" : "CG";
                warnings.Add($"Takeoff {envelopeUnit} is outside the envelope limits");
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
                var remainingFuelWeight = fuelWeight - fuelBurnWeight;

                // Recalculate fuel moment for remaining fuel using loading graph
                var (originalFuelMoment, _) = CalculateStationMoment(fuelStation, fuelWeight, profile.LoadingGraphFormat);
                var (remainingFuelMoment, _) = CalculateStationMoment(fuelStation, remainingFuelWeight, profile.LoadingGraphFormat);

                double landingWeight = totalWeight - fuelBurnWeight;
                double landingMoment = totalMoment - originalFuelMoment + remainingFuelMoment;
                double landingCgArm = landingWeight > 0 ? landingMoment / landingWeight : 0;

                // Determine the horizontal value for envelope check based on envelope format
                double landingHorizontalValue = envelope.Format == CgEnvelopeFormat.MomentDividedBy1000
                    ? landingMoment / 1000.0
                    : landingCgArm;

                bool landingWithinEnvelope = IsPointInEnvelope(landingWeight, landingHorizontalValue, envelope.Limits);

                // Check landing weight limits
                var maxLandingWeight = profile.MaxLandingWeight ?? profile.MaxTakeoffWeight;
                if (landingWeight > maxLandingWeight)
                {
                    warnings.Add($"Landing weight ({landingWeight:F1}) exceeds max landing weight ({maxLandingWeight:F1})");
                }

                if (!landingWithinEnvelope)
                {
                    var envelopeUnit = envelope.Format == CgEnvelopeFormat.MomentDividedBy1000 ? "Moment/1000" : "CG";
                    warnings.Add($"Landing {envelopeUnit} is outside the envelope limits");
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

    /// <summary>
    /// Calculates the moment for a station using loading graph interpolation.
    /// </summary>
    /// <param name="station">The loading station</param>
    /// <param name="weight">The weight at this station</param>
    /// <param name="format">The loading graph format (Arm or MomentDividedBy1000)</param>
    /// <returns>A tuple of (moment, arm)</returns>
    private static (double moment, double arm) CalculateStationMoment(LoadingStation station, double weight, LoadingGraphFormat format)
    {
        // Interpolate the value from the loading graph
        var interpolatedValue = station.InterpolateValue(weight);

        if (format == LoadingGraphFormat.MomentDividedBy1000)
        {
            // Loading graph gives us moment/1000, so moment = value * 1000
            var moment = interpolatedValue * 1000.0;
            var arm = weight > 0 ? moment / weight : 0;
            return (moment, arm);
        }
        else
        {
            // Loading graph gives us arm directly, so moment = weight * arm
            var arm = interpolatedValue;
            var moment = weight * arm;
            return (moment, arm);
        }
    }

    /// <summary>
    /// Determines if a point (weight, horizontalValue) is inside the envelope polygon.
    /// The horizontalValue is either CG arm or Moment/1000 depending on envelope format.
    /// Uses ray casting algorithm for point-in-polygon test.
    /// </summary>
    private static bool IsPointInEnvelope(double weight, double horizontalValue, List<CgEnvelopePoint> envelope)
    {
        if (envelope.Count < 3)
            return false;

        int n = envelope.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var pi = envelope[i];
            var pj = envelope[j];

            var piHorizontal = pi.HorizontalValue;
            var pjHorizontal = pj.HorizontalValue;

            // Ray casting: check if horizontal ray from point intersects edge
            if ((pi.Weight > weight) != (pj.Weight > weight) &&
                horizontalValue < (pjHorizontal - piHorizontal) * (weight - pi.Weight) / (pj.Weight - pi.Weight) + piHorizontal)
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public async Task<WeightBalanceCalculationDto> CalculateAndSave(string userId, SaveWeightBalanceCalculationRequestDto request)
    {
        try
        {
            // Perform the calculation using existing method
            var calculationRequest = new WeightBalanceCalculationRequestDto
            {
                LoadedStations = request.LoadedStations,
                EnvelopeId = request.EnvelopeId,
                FuelBurnGallons = request.FuelBurnGallons
            };

            var result = await Calculate(userId, request.ProfileId, calculationRequest);

            // If this is a flight-associated calculation, remove any existing calculation for this flight
            if (!string.IsNullOrEmpty(request.FlightId))
            {
                var existingFlightCalc = await _context.WeightBalanceCalculations
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.FlightId == request.FlightId);

                if (existingFlightCalc != null)
                {
                    _context.WeightBalanceCalculations.Remove(existingFlightCalc);
                }
            }
            else
            {
                // This is a standalone calculation - remove any previous standalone for this user
                var existingStandalone = await _context.WeightBalanceCalculations
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.IsStandalone);

                if (existingStandalone != null)
                {
                    _context.WeightBalanceCalculations.Remove(existingStandalone);
                }
            }

            // Create and save the new calculation
            var calculation = WeightBalanceCalculationMapper.CreateFromRequest(userId, request, result);

            _context.WeightBalanceCalculations.Add(calculation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Saved W&B calculation {CalculationId} for user {UserId}, flight {FlightId}",
                calculation.Id, userId, request.FlightId ?? "(standalone)");

            return WeightBalanceCalculationMapper.MapToDto(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving W&B calculation for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WeightBalanceCalculationDto?> GetCalculation(string userId, Guid calculationId)
    {
        try
        {
            var calculation = await _context.WeightBalanceCalculations
                .FirstOrDefaultAsync(c => c.Id == calculationId && c.UserId == userId);

            return calculation == null ? null : WeightBalanceCalculationMapper.MapToDto(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting W&B calculation {CalculationId} for user {UserId}", calculationId, userId);
            throw;
        }
    }

    public async Task<WeightBalanceCalculationDto?> GetCalculationForFlight(string userId, string flightId)
    {
        try
        {
            var calculation = await _context.WeightBalanceCalculations
                .FirstOrDefaultAsync(c => c.FlightId == flightId && c.UserId == userId);

            return calculation == null ? null : WeightBalanceCalculationMapper.MapToDto(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting W&B calculation for flight {FlightId} user {UserId}", flightId, userId);
            throw;
        }
    }

    public async Task<StandaloneCalculationStateDto?> GetLatestStandaloneState(string userId)
    {
        try
        {
            var calculation = await _context.WeightBalanceCalculations
                .Where(c => c.UserId == userId && c.IsStandalone)
                .OrderByDescending(c => c.CalculatedAt)
                .FirstOrDefaultAsync();

            return calculation == null ? null : WeightBalanceCalculationMapper.MapToStandaloneState(calculation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest standalone W&B state for user {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteCalculation(string userId, Guid calculationId)
    {
        try
        {
            var calculation = await _context.WeightBalanceCalculations
                .FirstOrDefaultAsync(c => c.Id == calculationId && c.UserId == userId);

            if (calculation == null)
            {
                throw new NotFoundException("WeightBalanceCalculation", calculationId);
            }

            _context.WeightBalanceCalculations.Remove(calculation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted W&B calculation {CalculationId} for user {UserId}", calculationId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting W&B calculation {CalculationId} for user {UserId}", calculationId, userId);
            throw;
        }
    }
}
