using FluentAssertions;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.WeightBalance;
using VFR3D.Infrastructure.Dtos.WeightBalance;
using VFR3D.Infrastructure.Mappers;
using Xunit;

namespace VFR3D.Tests.WeightAndBalanceTests;

/// <summary>
/// Tests for WeightBalanceCalculationMapper.
/// </summary>
public class WeightBalanceCalculationMapperTests
{
    private const string TestUserId = "test-user-123";
    private static readonly Guid TestProfileId = Guid.NewGuid();
    private const string TestFlightId = "flight-123";

    #region CreateFromRequest Tests

    [Fact]
    public void CreateFromRequest_ShouldCreateEntityWithCorrectValues()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.UserId.Should().Be(TestUserId);
        entity.WeightBalanceProfileId.Should().Be(TestProfileId);
        entity.FlightId.Should().Be(TestFlightId);
        entity.EnvelopeId.Should().Be("normal");
        entity.FuelBurnGallons.Should().Be(15);
        entity.IsStandalone.Should().BeFalse();
        entity.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateFromRequest_ShouldSetIsStandaloneTrue_WhenNoFlightId()
    {
        // Arrange
        var request = CreateSampleRequest();
        request = request with { FlightId = null };
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.IsStandalone.Should().BeTrue();
        entity.FlightId.Should().BeNull();
    }

    [Fact]
    public void CreateFromRequest_ShouldMapLoadedStations()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.LoadedStations.Should().HaveCount(2);
        entity.LoadedStations[0].StationId.Should().Be("front-seats");
        entity.LoadedStations[0].Weight.Should().Be(340);
        entity.LoadedStations[1].StationId.Should().Be("fuel");
        entity.LoadedStations[1].FuelGallons.Should().Be(40);
    }

    [Fact]
    public void CreateFromRequest_ShouldMapTakeoffResult()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.TakeoffResult.TotalWeight.Should().Be(2200);
        entity.TakeoffResult.TotalMoment.Should().Be(88000);
        entity.TakeoffResult.CgArm.Should().Be(40);
        entity.TakeoffResult.IsWithinEnvelope.Should().BeTrue();
    }

    [Fact]
    public void CreateFromRequest_ShouldMapLandingResult()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.LandingResult.Should().NotBeNull();
        entity.LandingResult!.TotalWeight.Should().Be(2110);
    }

    [Fact]
    public void CreateFromRequest_ShouldMapStationBreakdown()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.StationBreakdown.Should().HaveCount(3);
        entity.StationBreakdown[0].StationId.Should().Be("empty");
        entity.StationBreakdown[0].Name.Should().Be("Empty Weight");
    }

    [Fact]
    public void CreateFromRequest_ShouldMapEnvelopeInfo()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.EnvelopeName.Should().Be("Normal Category");
        entity.EnvelopeLimits.Should().HaveCount(4);
        entity.EnvelopeLimits[0].Weight.Should().Be(1500);
    }

    [Fact]
    public void CreateFromRequest_ShouldMapWarnings()
    {
        // Arrange
        var request = CreateSampleRequest();
        var result = CreateSampleResult();

        // Act
        var entity = WeightBalanceCalculationMapper.CreateFromRequest(TestUserId, request, result);

        // Assert
        entity.Warnings.Should().HaveCount(1);
        entity.Warnings[0].Should().Be("Test warning");
    }

    #endregion

    #region MapToDto Tests

    [Fact]
    public void MapToDto_ShouldMapAllFields()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dto = WeightBalanceCalculationMapper.MapToDto(entity);

        // Assert
        dto.Id.Should().Be(entity.Id);
        dto.ProfileId.Should().Be(entity.WeightBalanceProfileId);
        dto.FlightId.Should().Be(entity.FlightId);
        dto.EnvelopeId.Should().Be(entity.EnvelopeId);
        dto.FuelBurnGallons.Should().Be(entity.FuelBurnGallons);
        dto.IsStandalone.Should().Be(entity.IsStandalone);
        dto.CalculatedAt.Should().Be(entity.CalculatedAt);
    }

    [Fact]
    public void MapToDto_ShouldMapLoadedStations()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dto = WeightBalanceCalculationMapper.MapToDto(entity);

        // Assert
        dto.LoadedStations.Should().HaveCount(2);
        dto.LoadedStations[0].StationId.Should().Be("front-seats");
        dto.LoadedStations[0].Weight.Should().Be(340);
    }

    [Fact]
    public void MapToDto_ShouldMapResults()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dto = WeightBalanceCalculationMapper.MapToDto(entity);

        // Assert
        dto.Takeoff.TotalWeight.Should().Be(2200);
        dto.Takeoff.IsWithinEnvelope.Should().BeTrue();
        dto.Landing.Should().NotBeNull();
        dto.Landing!.TotalWeight.Should().Be(2110);
    }

    [Fact]
    public void MapToDto_ShouldMapStationBreakdown()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dto = WeightBalanceCalculationMapper.MapToDto(entity);

        // Assert
        dto.StationBreakdown.Should().HaveCount(3);
    }

    [Fact]
    public void MapToDto_ShouldMapEnvelopeInfo()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var dto = WeightBalanceCalculationMapper.MapToDto(entity);

        // Assert
        dto.EnvelopeName.Should().Be("Normal Category");
        dto.EnvelopeLimits.Should().HaveCount(4);
    }

    [Fact]
    public void MapToDto_ShouldHandleNullLandingResult()
    {
        // Arrange
        var entity = CreateSampleEntity();
        entity.LandingResult = null;

        // Act
        var dto = WeightBalanceCalculationMapper.MapToDto(entity);

        // Assert
        dto.Landing.Should().BeNull();
    }

    #endregion

    #region MapToStandaloneState Tests

    [Fact]
    public void MapToStandaloneState_ShouldMapInputsOnly()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var state = WeightBalanceCalculationMapper.MapToStandaloneState(entity);

        // Assert
        state.CalculationId.Should().Be(entity.Id);
        state.ProfileId.Should().Be(entity.WeightBalanceProfileId);
        state.EnvelopeId.Should().Be(entity.EnvelopeId);
        state.FuelBurnGallons.Should().Be(entity.FuelBurnGallons);
        state.CalculatedAt.Should().Be(entity.CalculatedAt);
    }

    [Fact]
    public void MapToStandaloneState_ShouldMapLoadedStations()
    {
        // Arrange
        var entity = CreateSampleEntity();

        // Act
        var state = WeightBalanceCalculationMapper.MapToStandaloneState(entity);

        // Assert
        state.LoadedStations.Should().HaveCount(2);
        state.LoadedStations[0].StationId.Should().Be("front-seats");
    }

    #endregion

    #region Test Data Helpers

    private static SaveWeightBalanceCalculationRequestDto CreateSampleRequest()
    {
        return new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = TestProfileId,
            FlightId = TestFlightId,
            EnvelopeId = "normal",
            FuelBurnGallons = 15,
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 40 }
            }
        };
    }

    private static WeightBalanceCalculationResultDto CreateSampleResult()
    {
        return new WeightBalanceCalculationResultDto
        {
            Takeoff = new WeightBalanceCgResultDto
            {
                TotalWeight = 2200,
                TotalMoment = 88000,
                CgArm = 40,
                IsWithinEnvelope = true
            },
            Landing = new WeightBalanceCgResultDto
            {
                TotalWeight = 2110,
                TotalMoment = 84400,
                CgArm = 40,
                IsWithinEnvelope = true
            },
            StationBreakdown = new List<StationBreakdownDto>
            {
                new StationBreakdownDto { StationId = "empty", Name = "Empty Weight", Weight = 1600, Arm = 40, Moment = 64000 },
                new StationBreakdownDto { StationId = "front-seats", Name = "Pilot & Passenger", Weight = 340, Arm = 37, Moment = 12580 },
                new StationBreakdownDto { StationId = "fuel", Name = "Fuel (40 gal)", Weight = 240, Arm = 48, Moment = 11520 }
            },
            EnvelopeName = "Normal Category",
            EnvelopeLimits = new List<CgEnvelopePointDto>
            {
                new CgEnvelopePointDto { Weight = 1500, MomentDividedBy1000 = 57 },
                new CgEnvelopePointDto { Weight = 2550, MomentDividedBy1000 = 94 },
                new CgEnvelopePointDto { Weight = 2550, MomentDividedBy1000 = 110 },
                new CgEnvelopePointDto { Weight = 1500, MomentDividedBy1000 = 70 }
            },
            Warnings = new List<string> { "Test warning" }
        };
    }

    private static WeightBalanceCalculation CreateSampleEntity()
    {
        return new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            WeightBalanceProfileId = TestProfileId,
            FlightId = TestFlightId,
            EnvelopeId = "normal",
            FuelBurnGallons = 15,
            IsStandalone = false,
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>
            {
                new StationLoad { StationId = "front-seats", Weight = 340 },
                new StationLoad { StationId = "fuel", FuelGallons = 40 }
            },
            TakeoffResult = new WeightBalanceCgResult
            {
                TotalWeight = 2200,
                TotalMoment = 88000,
                CgArm = 40,
                IsWithinEnvelope = true
            },
            LandingResult = new WeightBalanceCgResult
            {
                TotalWeight = 2110,
                TotalMoment = 84400,
                CgArm = 40,
                IsWithinEnvelope = true
            },
            StationBreakdown = new List<StationBreakdown>
            {
                new StationBreakdown { StationId = "empty", Name = "Empty Weight", Weight = 1600, Arm = 40, Moment = 64000 },
                new StationBreakdown { StationId = "front-seats", Name = "Pilot & Passenger", Weight = 340, Arm = 37, Moment = 12580 },
                new StationBreakdown { StationId = "fuel", Name = "Fuel (40 gal)", Weight = 240, Arm = 48, Moment = 11520 }
            },
            EnvelopeName = "Normal Category",
            EnvelopeLimits = new List<CgEnvelopePoint>
            {
                new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 57 },
                new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 94 },
                new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 110 },
                new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 70 }
            },
            Warnings = new List<string> { "Test warning" }
        };
    }

    #endregion
}
