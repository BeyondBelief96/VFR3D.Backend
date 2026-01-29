using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.WeightBalance;
using VFR3D.Infrastructure.Dtos.Mappers;
using VFR3D.Infrastructure.Dtos.WeightBalance;

namespace VFR3D.Infrastructure.Mappers;

public static class WeightBalanceCalculationMapper
{
    /// <summary>
    /// Creates a WeightBalanceCalculation entity from a request and calculation result.
    /// </summary>
    public static WeightBalanceCalculation CreateFromRequest(
        string userId,
        SaveWeightBalanceCalculationRequestDto request,
        WeightBalanceCalculationResultDto result)
    {
        return new WeightBalanceCalculation
        {
            UserId = userId,
            FlightId = request.FlightId,
            WeightBalanceProfileId = request.ProfileId,
            EnvelopeId = request.EnvelopeId,
            FuelBurnGallons = request.FuelBurnGallons,
            LoadedStations = request.LoadedStations.Select(MapStationLoadFromDto).ToList(),
            TakeoffResult = MapCgResultFromDto(result.Takeoff),
            LandingResult = result.Landing != null ? MapCgResultFromDto(result.Landing) : null,
            StationBreakdown = result.StationBreakdown.Select(MapStationBreakdownFromDto).ToList(),
            EnvelopeName = result.EnvelopeName,
            EnvelopeLimits = result.EnvelopeLimits.Select(MapEnvelopePointFromDto).ToList(),
            Warnings = result.Warnings.ToList(),
            CalculatedAt = DateTime.UtcNow,
            IsStandalone = string.IsNullOrEmpty(request.FlightId)
        };
    }

    /// <summary>
    /// Maps a WeightBalanceCalculation entity to its full DTO.
    /// </summary>
    public static WeightBalanceCalculationDto MapToDto(WeightBalanceCalculation entity)
    {
        return new WeightBalanceCalculationDto
        {
            Id = entity.Id,
            ProfileId = entity.WeightBalanceProfileId,
            FlightId = entity.FlightId,
            EnvelopeId = entity.EnvelopeId,
            FuelBurnGallons = entity.FuelBurnGallons,
            LoadedStations = entity.LoadedStations.Select(MapStationLoadToDto).ToList(),
            Takeoff = MapCgResultToDto(entity.TakeoffResult),
            Landing = entity.LandingResult != null ? MapCgResultToDto(entity.LandingResult) : null,
            StationBreakdown = entity.StationBreakdown.Select(MapStationBreakdownToDto).ToList(),
            EnvelopeName = entity.EnvelopeName,
            EnvelopeLimits = entity.EnvelopeLimits.Select(WeightBalanceProfileMapper.MapEnvelopePointToDto).ToList(),
            Warnings = entity.Warnings.ToList(),
            CalculatedAt = entity.CalculatedAt,
            IsStandalone = entity.IsStandalone
        };
    }

    /// <summary>
    /// Maps a WeightBalanceCalculation entity to the lightweight standalone state DTO for form repopulation.
    /// </summary>
    public static StandaloneCalculationStateDto MapToStandaloneState(WeightBalanceCalculation entity)
    {
        return new StandaloneCalculationStateDto
        {
            CalculationId = entity.Id,
            ProfileId = entity.WeightBalanceProfileId,
            EnvelopeId = entity.EnvelopeId,
            FuelBurnGallons = entity.FuelBurnGallons,
            LoadedStations = entity.LoadedStations.Select(MapStationLoadToDto).ToList(),
            CalculatedAt = entity.CalculatedAt
        };
    }

    private static StationLoad MapStationLoadFromDto(StationLoadDto dto)
    {
        return new StationLoad
        {
            StationId = dto.StationId,
            Weight = dto.Weight,
            FuelGallons = dto.FuelGallons,
            OilQuarts = dto.OilQuarts
        };
    }

    private static StationLoadDto MapStationLoadToDto(StationLoad entity)
    {
        return new StationLoadDto
        {
            StationId = entity.StationId,
            Weight = entity.Weight,
            FuelGallons = entity.FuelGallons,
            OilQuarts = entity.OilQuarts
        };
    }

    private static WeightBalanceCgResult MapCgResultFromDto(WeightBalanceCgResultDto dto)
    {
        return new WeightBalanceCgResult
        {
            TotalWeight = dto.TotalWeight,
            TotalMoment = dto.TotalMoment,
            CgArm = dto.CgArm,
            IsWithinEnvelope = dto.IsWithinEnvelope
        };
    }

    private static WeightBalanceCgResultDto MapCgResultToDto(WeightBalanceCgResult entity)
    {
        return new WeightBalanceCgResultDto
        {
            TotalWeight = entity.TotalWeight,
            TotalMoment = entity.TotalMoment,
            CgArm = entity.CgArm,
            IsWithinEnvelope = entity.IsWithinEnvelope
        };
    }

    private static StationBreakdown MapStationBreakdownFromDto(StationBreakdownDto dto)
    {
        return new StationBreakdown
        {
            StationId = dto.StationId,
            Name = dto.Name,
            Weight = dto.Weight,
            Arm = dto.Arm,
            Moment = dto.Moment
        };
    }

    private static StationBreakdownDto MapStationBreakdownToDto(StationBreakdown entity)
    {
        return new StationBreakdownDto
        {
            StationId = entity.StationId,
            Name = entity.Name,
            Weight = entity.Weight,
            Arm = entity.Arm,
            Moment = entity.Moment
        };
    }

    private static CgEnvelopePoint MapEnvelopePointFromDto(CgEnvelopePointDto dto)
    {
        return new CgEnvelopePoint
        {
            Weight = dto.Weight,
            Arm = dto.Arm,
            MomentDividedBy1000 = dto.MomentDividedBy1000
        };
    }
}
