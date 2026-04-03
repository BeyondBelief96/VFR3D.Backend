using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.WeightBalance;
using VFR3D.Infrastructure.Dtos.WeightBalance;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class WeightBalanceProfileMapper
{
    public static WeightBalanceProfileDto MapToDto(WeightBalanceProfile entity)
    {
        return new WeightBalanceProfileDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AircraftId = entity.AircraftId,
            ProfileName = entity.ProfileName,
            DatumDescription = entity.DatumDescription,
            EmptyWeight = entity.EmptyWeight,
            EmptyWeightArm = entity.EmptyWeightArm,
            MaxRampWeight = entity.MaxRampWeight,
            MaxTakeoffWeight = entity.MaxTakeoffWeight,
            MaxLandingWeight = entity.MaxLandingWeight,
            MaxZeroFuelWeight = entity.MaxZeroFuelWeight,
            WeightUnits = entity.WeightUnits,
            ArmUnits = entity.ArmUnits,
            LoadingGraphFormat = entity.LoadingGraphFormat,
            LoadingStations = entity.LoadingStations.Select(MapStationToDto).ToList(),
            CgEnvelopes = entity.CgEnvelopes.Select(MapEnvelopeToDto).ToList()
        };
    }

    public static WeightBalanceProfile CreateFromRequest(string userId, CreateWeightBalanceProfileRequestDto request)
    {
        return new WeightBalanceProfile
        {
            UserId = userId,
            AircraftId = request.AircraftId,
            ProfileName = request.ProfileName,
            DatumDescription = request.DatumDescription,
            EmptyWeight = request.EmptyWeight,
            EmptyWeightArm = request.EmptyWeightArm,
            MaxRampWeight = request.MaxRampWeight,
            MaxTakeoffWeight = request.MaxTakeoffWeight,
            MaxLandingWeight = request.MaxLandingWeight,
            MaxZeroFuelWeight = request.MaxZeroFuelWeight,
            WeightUnits = request.WeightUnits,
            ArmUnits = request.ArmUnits,
            LoadingGraphFormat = request.LoadingGraphFormat,
            LoadingStations = request.LoadingStations.Select(MapStationFromDto).ToList(),
            CgEnvelopes = request.CgEnvelopes.Select(MapEnvelopeFromDto).ToList()
        };
    }

    public static void UpdateFromRequest(WeightBalanceProfile entity, UpdateWeightBalanceProfileRequestDto request)
    {
        entity.AircraftId = request.AircraftId;
        entity.ProfileName = request.ProfileName;
        entity.DatumDescription = request.DatumDescription;
        entity.EmptyWeight = request.EmptyWeight;
        entity.EmptyWeightArm = request.EmptyWeightArm;
        entity.MaxRampWeight = request.MaxRampWeight;
        entity.MaxTakeoffWeight = request.MaxTakeoffWeight;
        entity.MaxLandingWeight = request.MaxLandingWeight;
        entity.MaxZeroFuelWeight = request.MaxZeroFuelWeight;
        entity.WeightUnits = request.WeightUnits;
        entity.ArmUnits = request.ArmUnits;
        entity.LoadingGraphFormat = request.LoadingGraphFormat;
        entity.LoadingStations = request.LoadingStations.Select(MapStationFromDto).ToList();
        entity.CgEnvelopes = request.CgEnvelopes.Select(MapEnvelopeFromDto).ToList();
    }

    private static LoadingStationDto MapStationToDto(LoadingStation station)
    {
        return new LoadingStationDto
        {
            Id = station.Id,
            Name = station.Name,
            MaxWeight = station.MaxWeight,
            Point1 = MapLoadingGraphPointToDto(station.Point1),
            Point2 = MapLoadingGraphPointToDto(station.Point2),
            StationType = station.StationType,
            FuelCapacityGallons = station.FuelCapacityGallons,
            FuelWeightPerGallon = station.FuelWeightPerGallon,
            OilCapacityQuarts = station.OilCapacityQuarts,
            OilWeightPerQuart = station.OilWeightPerQuart
        };
    }

    private static LoadingStation MapStationFromDto(LoadingStationDto dto)
    {
        return new LoadingStation
        {
            Id = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id,
            Name = dto.Name,
            MaxWeight = dto.MaxWeight,
            Point1 = MapLoadingGraphPointFromDto(dto.Point1),
            Point2 = MapLoadingGraphPointFromDto(dto.Point2),
            StationType = dto.StationType,
            FuelCapacityGallons = dto.FuelCapacityGallons,
            FuelWeightPerGallon = dto.FuelWeightPerGallon,
            OilCapacityQuarts = dto.OilCapacityQuarts,
            OilWeightPerQuart = dto.OilWeightPerQuart
        };
    }

    private static LoadingGraphPointDto MapLoadingGraphPointToDto(LoadingGraphPoint point)
    {
        return new LoadingGraphPointDto
        {
            Weight = point.Weight,
            Value = point.Value
        };
    }

    private static LoadingGraphPoint MapLoadingGraphPointFromDto(LoadingGraphPointDto dto)
    {
        return new LoadingGraphPoint
        {
            Weight = dto.Weight,
            Value = dto.Value
        };
    }

    private static CgEnvelopeDto MapEnvelopeToDto(CgEnvelope envelope)
    {
        return new CgEnvelopeDto
        {
            Id = envelope.Id,
            Name = envelope.Name,
            Format = envelope.Format,
            Limits = envelope.Limits.Select(MapEnvelopePointToDto).ToList()
        };
    }

    private static CgEnvelope MapEnvelopeFromDto(CgEnvelopeDto dto)
    {
        return new CgEnvelope
        {
            Id = string.IsNullOrWhiteSpace(dto.Id) ? Guid.NewGuid().ToString() : dto.Id,
            Name = dto.Name,
            Format = dto.Format,
            Limits = dto.Limits.Select(MapEnvelopePointFromDto).ToList()
        };
    }

    public static CgEnvelopePointDto MapEnvelopePointToDto(CgEnvelopePoint point)
    {
        return new CgEnvelopePointDto
        {
            Weight = point.Weight,
            Arm = point.Arm,
            MomentDividedBy1000 = point.MomentDividedBy1000
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
