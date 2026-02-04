using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;
using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;
using VFR3D.Domain.ValueObjects.WeightBalance;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Dtos.WeightBalance;
using VFR3D.Infrastructure.Services;
using VFR3D.Domain.Exceptions;
using Xunit;

namespace VFR3D.Tests.WeightAndBalanceTests;

/// <summary>
/// Tests for WeightBalanceProfileService calculation persistence methods.
/// </summary>
public class WeightBalanceCalculationServiceTests
{
    private readonly VFR3DDbContext _context;
    private readonly WeightBalanceProfileService _service;
    private readonly ILogger<WeightBalanceProfileService> _logger;

    private const string TestUserId = "test-user-123";

    public WeightBalanceCalculationServiceTests()
    {
        _logger = Substitute.For<ILogger<WeightBalanceProfileService>>();
        _context = CreateMockDbContext();
        _service = new WeightBalanceProfileService(_context, _logger);
    }

    private VFR3DDbContext CreateMockDbContext()
    {
        var mockContext = Substitute.For<VFR3DDbContext>(
            new DbContextOptionsBuilder<VFR3DDbContext>().Options);
        return mockContext;
    }

    private void SetupMockProfiles(params WeightBalanceProfile[] profiles)
    {
        var mockDbSet = profiles.AsQueryable().BuildMockDbSet();
        _context.WeightBalanceProfiles.Returns(mockDbSet);
    }

    private void SetupMockCalculations(params WeightBalanceCalculation[] calculations)
    {
        var mockDbSet = calculations.AsQueryable().BuildMockDbSet();
        _context.WeightBalanceCalculations.Returns(mockDbSet);
    }

    private WeightBalanceProfile CreateTestProfile()
    {
        return new WeightBalanceProfile
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            ProfileName = "Test Profile",
            EmptyWeight = 1600,
            EmptyWeightArm = 40,
            MaxTakeoffWeight = 2550,
            LoadingGraphFormat = LoadingGraphFormat.MomentDividedBy1000,
            LoadingStations = new List<LoadingStation>
            {
                new LoadingStation
                {
                    Id = "front-seats",
                    Name = "Front Seats",
                    MaxWeight = 400,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 }
                },
                new LoadingStation
                {
                    Id = "fuel",
                    Name = "Fuel",
                    MaxWeight = 318,
                    StationType = LoadingStationType.Fuel,
                    FuelCapacityGallons = 53,
                    FuelWeightPerGallon = 6.0,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 318, Value = 15.264 }
                }
            },
            CgEnvelopes = new List<CgEnvelope>
            {
                new CgEnvelope
                {
                    Id = "normal",
                    Name = "Normal Category",
                    Format = CgEnvelopeFormat.MomentDividedBy1000,
                    Limits = new List<CgEnvelopePoint>
                    {
                        new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 57 },
                        new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 94 },
                        new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 110 },
                        new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 70 }
                    }
                }
            }
        };
    }

    #region CalculateAndSave Tests

    [Fact]
    public async Task CalculateAndSave_ShouldCalculateAndPersist_StandaloneCalculation()
    {
        // Arrange
        var profile = CreateTestProfile();
        SetupMockProfiles(profile);
        SetupMockCalculations();

        var request = new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = profile.Id,
            FlightId = null, // Standalone
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 40 }
            }
        };

        // Act
        var result = await _service.CalculateAndSave(TestUserId, request);

        // Assert
        result.Should().NotBeNull();
        result.IsStandalone.Should().BeTrue();
        result.FlightId.Should().BeNull();
        result.ProfileId.Should().Be(profile.Id);
        result.Takeoff.Should().NotBeNull();
        result.Takeoff.TotalWeight.Should().BeGreaterThan(0);

        // Verify entity was added
        _context.WeightBalanceCalculations.Received(1).Add(Arg.Any<WeightBalanceCalculation>());
        await _context.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CalculateAndSave_ShouldCalculateAndPersist_FlightAssociatedCalculation()
    {
        // Arrange
        var profile = CreateTestProfile();
        SetupMockProfiles(profile);
        SetupMockCalculations();

        var flightId = "flight-123";
        var request = new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = profile.Id,
            FlightId = flightId,
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 40 }
            }
        };

        // Act
        var result = await _service.CalculateAndSave(TestUserId, request);

        // Assert
        result.Should().NotBeNull();
        result.IsStandalone.Should().BeFalse();
        result.FlightId.Should().Be(flightId);
    }

    [Fact]
    public async Task CalculateAndSave_ShouldIncludeLandingResult_WhenFuelBurnProvided()
    {
        // Arrange
        var profile = CreateTestProfile();
        SetupMockProfiles(profile);
        SetupMockCalculations();

        var request = new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = profile.Id,
            FuelBurnGallons = 15,
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 40 }
            }
        };

        // Act
        var result = await _service.CalculateAndSave(TestUserId, request);

        // Assert
        result.Landing.Should().NotBeNull();
        result.Landing!.TotalWeight.Should().BeLessThan(result.Takeoff.TotalWeight);
    }

    [Fact]
    public async Task CalculateAndSave_ShouldRemovePreviousStandalone_WhenSavingNewStandalone()
    {
        // Arrange
        var profile = CreateTestProfile();
        SetupMockProfiles(profile);

        var existingStandalone = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            WeightBalanceProfileId = profile.Id,
            IsStandalone = true,
            CalculatedAt = DateTime.UtcNow.AddHours(-1),
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(existingStandalone);

        var request = new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = profile.Id,
            FlightId = null, // Standalone
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 }
            }
        };

        // Act
        var result = await _service.CalculateAndSave(TestUserId, request);

        // Assert
        _context.WeightBalanceCalculations.Received(1).Remove(existingStandalone);
    }

    [Fact]
    public async Task CalculateAndSave_ShouldRemovePreviousFlightCalculation_WhenSavingForSameFlight()
    {
        // Arrange
        var profile = CreateTestProfile();
        SetupMockProfiles(profile);

        var flightId = "flight-123";
        var existingFlightCalc = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            FlightId = flightId,
            WeightBalanceProfileId = profile.Id,
            IsStandalone = false,
            CalculatedAt = DateTime.UtcNow.AddHours(-1),
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(existingFlightCalc);

        var request = new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = profile.Id,
            FlightId = flightId,
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 }
            }
        };

        // Act
        var result = await _service.CalculateAndSave(TestUserId, request);

        // Assert
        _context.WeightBalanceCalculations.Received(1).Remove(existingFlightCalc);
    }

    [Fact]
    public async Task CalculateAndSave_ShouldThrow_WhenProfileNotFound()
    {
        // Arrange
        SetupMockProfiles(); // No profiles
        SetupMockCalculations();

        var request = new SaveWeightBalanceCalculationRequestDto
        {
            ProfileId = Guid.NewGuid(),
            LoadedStations = new List<StationLoadDto>()
        };

        // Act
        Func<Task> act = async () => await _service.CalculateAndSave(TestUserId, request);

        // Assert
        await act.Should().ThrowAsync<WeightBalanceProfileNotFoundException>();
    }

    #endregion

    #region GetCalculation Tests

    [Fact]
    public async Task GetCalculation_ShouldReturnCalculation_WhenExists()
    {
        // Arrange
        var calculationId = Guid.NewGuid();
        var calculation = new WeightBalanceCalculation
        {
            Id = calculationId,
            UserId = TestUserId,
            WeightBalanceProfileId = Guid.NewGuid(),
            IsStandalone = true,
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>
            {
                new StationLoad { StationId = "front-seats", Weight = 340 }
            },
            TakeoffResult = new WeightBalanceCgResult { TotalWeight = 2000, TotalMoment = 80000, CgArm = 40, IsWithinEnvelope = true },
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeName = "Normal",
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(calculation);

        // Act
        var result = await _service.GetCalculation(TestUserId, calculationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(calculationId);
    }

    [Fact]
    public async Task GetCalculation_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        SetupMockCalculations();

        // Act
        var result = await _service.GetCalculation(TestUserId, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCalculation_ShouldReturnNull_WhenWrongUser()
    {
        // Arrange
        var calculation = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = "other-user",
            WeightBalanceProfileId = Guid.NewGuid(),
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(calculation);

        // Act
        var result = await _service.GetCalculation(TestUserId, calculation.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetCalculationForFlight Tests

    [Fact]
    public async Task GetCalculationForFlight_ShouldReturnCalculation_WhenExists()
    {
        // Arrange
        var flightId = "flight-123";
        var calculation = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            FlightId = flightId,
            WeightBalanceProfileId = Guid.NewGuid(),
            IsStandalone = false,
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult { TotalWeight = 2000 },
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(calculation);

        // Act
        var result = await _service.GetCalculationForFlight(TestUserId, flightId);

        // Assert
        result.Should().NotBeNull();
        result!.FlightId.Should().Be(flightId);
    }

    [Fact]
    public async Task GetCalculationForFlight_ShouldReturnNull_WhenNoCalculationForFlight()
    {
        // Arrange
        SetupMockCalculations();

        // Act
        var result = await _service.GetCalculationForFlight(TestUserId, "non-existent-flight");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetLatestStandaloneState Tests

    [Fact]
    public async Task GetLatestStandaloneState_ShouldReturnLatest_WhenMultipleExist()
    {
        // Arrange
        var older = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            WeightBalanceProfileId = Guid.NewGuid(),
            IsStandalone = true,
            CalculatedAt = DateTime.UtcNow.AddHours(-2),
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        var newer = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            WeightBalanceProfileId = Guid.NewGuid(),
            IsStandalone = true,
            CalculatedAt = DateTime.UtcNow.AddHours(-1),
            LoadedStations = new List<StationLoad>
            {
                new StationLoad { StationId = "front-seats", Weight = 340 }
            },
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(older, newer);

        // Act
        var result = await _service.GetLatestStandaloneState(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result!.CalculationId.Should().Be(newer.Id);
    }

    [Fact]
    public async Task GetLatestStandaloneState_ShouldReturnNull_WhenNoStandaloneExists()
    {
        // Arrange
        var flightCalculation = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            FlightId = "flight-123",
            WeightBalanceProfileId = Guid.NewGuid(),
            IsStandalone = false, // Not standalone
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(flightCalculation);

        // Act
        var result = await _service.GetLatestStandaloneState(TestUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestStandaloneState_ShouldReturnInputsForFormRepopulation()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        var calculation = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            WeightBalanceProfileId = profileId,
            EnvelopeId = "normal",
            FuelBurnGallons = 15,
            IsStandalone = true,
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>
            {
                new StationLoad { StationId = "front-seats", Weight = 340 },
                new StationLoad { StationId = "fuel", FuelGallons = 40 }
            },
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(calculation);

        // Act
        var result = await _service.GetLatestStandaloneState(TestUserId);

        // Assert
        result.Should().NotBeNull();
        result!.ProfileId.Should().Be(profileId);
        result.EnvelopeId.Should().Be("normal");
        result.FuelBurnGallons.Should().Be(15);
        result.LoadedStations.Should().HaveCount(2);
        result.LoadedStations[0].StationId.Should().Be("front-seats");
        result.LoadedStations[0].Weight.Should().Be(340);
        result.LoadedStations[1].StationId.Should().Be("fuel");
        result.LoadedStations[1].FuelGallons.Should().Be(40);
    }

    #endregion

    #region DeleteCalculation Tests

    [Fact]
    public async Task DeleteCalculation_ShouldRemoveCalculation_WhenExists()
    {
        // Arrange
        var calculationId = Guid.NewGuid();
        var calculation = new WeightBalanceCalculation
        {
            Id = calculationId,
            UserId = TestUserId,
            WeightBalanceProfileId = Guid.NewGuid(),
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(calculation);

        // Act
        await _service.DeleteCalculation(TestUserId, calculationId);

        // Assert
        _context.WeightBalanceCalculations.Received(1).Remove(calculation);
        await _context.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteCalculation_ShouldThrow_WhenNotFound()
    {
        // Arrange
        SetupMockCalculations();

        // Act
        Func<Task> act = async () => await _service.DeleteCalculation(TestUserId, Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteCalculation_ShouldThrow_WhenWrongUser()
    {
        // Arrange
        var calculation = new WeightBalanceCalculation
        {
            Id = Guid.NewGuid(),
            UserId = "other-user",
            WeightBalanceProfileId = Guid.NewGuid(),
            CalculatedAt = DateTime.UtcNow,
            LoadedStations = new List<StationLoad>(),
            TakeoffResult = new WeightBalanceCgResult(),
            StationBreakdown = new List<StationBreakdown>(),
            EnvelopeLimits = new List<CgEnvelopePoint>(),
            Warnings = new List<string>()
        };

        SetupMockCalculations(calculation);

        // Act
        Func<Task> act = async () => await _service.DeleteCalculation(TestUserId, calculation.Id);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
