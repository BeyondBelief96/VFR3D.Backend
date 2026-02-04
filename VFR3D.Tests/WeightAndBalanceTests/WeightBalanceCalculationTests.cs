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
/// Comprehensive test suite for Weight & Balance calculations.
/// These tests are critical as W&B miscalculations can lead to loss of aircraft control.
/// Test data is based on real Cessna 172S POH values for accuracy.
/// </summary>
public class WeightBalanceCalculationTests
{
    private readonly VFR3DDbContext _context;
    private readonly WeightBalanceProfileService _service;
    private readonly ILogger<WeightBalanceProfileService> _logger;

    // Test user ID
    private const string TestUserId = "test-user-123";

    public WeightBalanceCalculationTests()
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

    #region Test Data Builders

    /// <summary>
    /// Creates a realistic Cessna 172S weight and balance profile using Moment/1000 format.
    /// Based on actual POH data for accurate testing.
    /// </summary>
    private WeightBalanceProfile CreateCessna172SProfile_MomentFormat()
    {
        return new WeightBalanceProfile
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            ProfileName = "Cessna 172S - Moment Format",
            DatumDescription = "Forward face of firewall",
            EmptyWeight = 1663,
            EmptyWeightArm = 40.5, // Results in empty moment of 67351.5
            MaxRampWeight = 2558,
            MaxTakeoffWeight = 2550,
            MaxLandingWeight = 2550,
            WeightUnits = WeightUnits.Pounds,
            ArmUnits = ArmUnits.Inches,
            LoadingGraphFormat = LoadingGraphFormat.MomentDividedBy1000,
            LoadingStations = new List<LoadingStation>
            {
                // Pilot & Front Passenger - Arm 37"
                new LoadingStation
                {
                    Id = "front-seats",
                    Name = "Pilot & Front Passenger",
                    MaxWeight = 400,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 } // 400 * 37 / 1000 = 14.8
                },
                // Rear Passengers - Arm 73"
                new LoadingStation
                {
                    Id = "rear-seats",
                    Name = "Rear Passengers",
                    MaxWeight = 400,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 400, Value = 29.2 } // 400 * 73 / 1000 = 29.2
                },
                // Baggage Area 1 - Arm 95"
                new LoadingStation
                {
                    Id = "baggage-1",
                    Name = "Baggage Area 1",
                    MaxWeight = 120,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 120, Value = 11.4 } // 120 * 95 / 1000 = 11.4
                },
                // Fuel (53 gal usable) - Arm 48"
                new LoadingStation
                {
                    Id = "fuel",
                    Name = "Fuel",
                    MaxWeight = 318, // 53 gal * 6 lbs/gal
                    StationType = LoadingStationType.Fuel,
                    FuelCapacityGallons = 53,
                    FuelWeightPerGallon = 6.0,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 318, Value = 15.264 } // 318 * 48 / 1000 = 15.264
                }
            },
            CgEnvelopes = new List<CgEnvelope>
            {
                // Normal Category CG Envelope (Moment/1000 format)
                // Based on realistic Cessna 172S POH envelope
                // Forward limit: more restrictive at low weights, opens up at higher weights
                // Aft limit: follows a line from low weight to max weight
                new CgEnvelope
                {
                    Id = "normal",
                    Name = "Normal Category",
                    Format = CgEnvelopeFormat.MomentDividedBy1000,
                    Limits = new List<CgEnvelopePoint>
                    {
                        // Forward limit (left side of envelope, traced bottom to top)
                        new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 57.0 },  // Min weight, forward limit
                        new CgEnvelopePoint { Weight = 1950, MomentDividedBy1000 = 73.0 },  // Utility weight, forward limit
                        new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 94.0 },  // Max weight, forward limit
                        // Aft limit (right side of envelope, traced top to bottom)
                        new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 110.0 }, // Max weight, aft limit
                        new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 70.0 }   // Min weight, aft limit
                    }
                }
            }
        };
    }

    /// <summary>
    /// Creates a weight and balance profile using Arm format instead of Moment/1000.
    /// </summary>
    private WeightBalanceProfile CreateProfile_ArmFormat()
    {
        return new WeightBalanceProfile
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            ProfileName = "Test Aircraft - Arm Format",
            EmptyWeight = 1500,
            EmptyWeightArm = 38.0,
            MaxTakeoffWeight = 2400,
            MaxLandingWeight = 2400,
            WeightUnits = WeightUnits.Pounds,
            ArmUnits = ArmUnits.Inches,
            LoadingGraphFormat = LoadingGraphFormat.Arm,
            LoadingStations = new List<LoadingStation>
            {
                new LoadingStation
                {
                    Id = "front-seats",
                    Name = "Front Seats",
                    MaxWeight = 400,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 37.0 },
                    Point2 = new LoadingGraphPoint { Weight = 400, Value = 37.0 } // Constant arm
                },
                new LoadingStation
                {
                    Id = "fuel",
                    Name = "Fuel",
                    MaxWeight = 240,
                    StationType = LoadingStationType.Fuel,
                    FuelCapacityGallons = 40,
                    FuelWeightPerGallon = 6.0,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 48.0 },
                    Point2 = new LoadingGraphPoint { Weight = 240, Value = 48.0 } // Constant arm
                }
            },
            CgEnvelopes = new List<CgEnvelope>
            {
                new CgEnvelope
                {
                    Id = "normal",
                    Name = "Normal",
                    Format = CgEnvelopeFormat.Arm,
                    Limits = new List<CgEnvelopePoint>
                    {
                        new CgEnvelopePoint { Weight = 1400, Arm = 35.0 },
                        new CgEnvelopePoint { Weight = 2400, Arm = 35.0 },
                        new CgEnvelopePoint { Weight = 2400, Arm = 47.0 },
                        new CgEnvelopePoint { Weight = 1400, Arm = 47.0 }
                    }
                }
            }
        };
    }

    #endregion

    #region Loading Graph Interpolation Tests

    [Fact]
    public void InterpolateValue_ShouldReturnCorrectValue_AtPoint1()
    {
        // Arrange
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
            Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 }
        };

        // Act
        var result = station.InterpolateValue(0);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void InterpolateValue_ShouldReturnCorrectValue_AtPoint2()
    {
        // Arrange
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
            Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 }
        };

        // Act
        var result = station.InterpolateValue(400);

        // Assert
        result.Should().BeApproximately(14.8, 0.001);
    }

    [Fact]
    public void InterpolateValue_ShouldReturnCorrectValue_AtMidpoint()
    {
        // Arrange - Front seats with arm of 37"
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
            Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 }
        };

        // Act - 200 lbs (midpoint)
        var result = station.InterpolateValue(200);

        // Assert - Should be 7.4 (200 * 37 / 1000)
        result.Should().BeApproximately(7.4, 0.01);
    }

    [Fact]
    public void InterpolateValue_ShouldReturnCorrectValue_ForTypicalPilotWeight()
    {
        // Arrange - Front seats with arm of 37"
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
            Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 }
        };

        // Act - 170 lb pilot
        var result = station.InterpolateValue(170);

        // Assert - Should be 6.29 (170 * 37 / 1000)
        result.Should().BeApproximately(6.29, 0.01);
    }

    [Fact]
    public void InterpolateValue_ShouldHandleConstantArm()
    {
        // Arrange - Station with constant arm (vertical line on loading graph)
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 0, Value = 37.0 },
            Point2 = new LoadingGraphPoint { Weight = 400, Value = 37.0 }
        };

        // Act
        var result = station.InterpolateValue(200);

        // Assert - Should always be 37.0 regardless of weight
        result.Should().Be(37.0);
    }

    [Fact]
    public void InterpolateValue_ShouldExtrapolateBeyondPoint2()
    {
        // Arrange
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
            Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 }
        };

        // Act - Weight beyond defined range
        var result = station.InterpolateValue(500);

        // Assert - Should extrapolate linearly: 500 * 37 / 1000 = 18.5
        result.Should().BeApproximately(18.5, 0.01);
    }

    [Fact]
    public void InterpolateValue_ShouldHandleSameWeightPoints()
    {
        // Arrange - Edge case: both points have same weight
        var station = new LoadingStation
        {
            Point1 = new LoadingGraphPoint { Weight = 100, Value = 5.0 },
            Point2 = new LoadingGraphPoint { Weight = 100, Value = 5.0 }
        };

        // Act
        var result = station.InterpolateValue(100);

        // Assert - Should return Point1.Value to avoid division by zero
        result.Should().Be(5.0);
    }

    #endregion

    #region Full Calculation Tests - Moment/1000 Format

    [Fact]
    public async Task Calculate_ShouldReturnCorrectResults_ForTypicalLoading_MomentFormat()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 }, // Pilot + passenger
                new StationLoadDto { StationId = "rear-seats", Weight = 0 },
                new StationLoadDto { StationId = "baggage-1", Weight = 20 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 53 } // Full fuel
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Takeoff.Should().NotBeNull();

        // Verify weight calculation:
        // Empty: 1663 lbs
        // Front seats: 340 lbs
        // Rear seats: 0 lbs
        // Baggage: 20 lbs
        // Fuel: 53 * 6 = 318 lbs
        // Total: 2341 lbs
        result.Takeoff.TotalWeight.Should().BeApproximately(2341, 1);

        // Verify CG is within envelope
        result.Takeoff.IsWithinEnvelope.Should().BeTrue();
        result.Warnings.Should().NotContain(w => w.Contains("outside the envelope"));
    }

    [Fact]
    public async Task Calculate_ShouldDetectOverweightCondition()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 400 }, // Max front seats
                new StationLoadDto { StationId = "rear-seats", Weight = 400 }, // Max rear seats
                new StationLoadDto { StationId = "baggage-1", Weight = 120 }, // Max baggage
                new StationLoadDto { StationId = "fuel", FuelGallons = 53 } // Full fuel
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        // Total: 1663 + 400 + 400 + 120 + 318 = 2901 lbs (over 2550 max)
        result.Takeoff.TotalWeight.Should().BeApproximately(2901, 1);
        result.Warnings.Should().Contain(w => w.Contains("exceeds max takeoff weight"));
    }

    [Fact]
    public async Task Calculate_ShouldDetectAftCgCondition()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        // Heavy rear loading with light front = aft CG
        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 150 }, // Light pilot only
                new StationLoadDto { StationId = "rear-seats", Weight = 400 }, // Max rear passengers
                new StationLoadDto { StationId = "baggage-1", Weight = 120 }, // Max baggage
                new StationLoadDto { StationId = "fuel", FuelGallons = 30 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert - CG should be aft of limits
        result.Takeoff.IsWithinEnvelope.Should().BeFalse();
        result.Warnings.Should().Contain(w => w.Contains("outside the envelope"));
    }

    [Fact]
    public async Task Calculate_ShouldCalculateLandingCondition_WithFuelBurn()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "rear-seats", Weight = 100 },
                new StationLoadDto { StationId = "baggage-1", Weight = 20 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 53 }
            },
            FuelBurnGallons = 20 // Burn 20 gallons during flight
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Landing.Should().NotBeNull();

        // Landing weight should be takeoff minus fuel burn
        // Fuel burn: 20 * 6 = 120 lbs
        var expectedLandingWeight = result.Takeoff.TotalWeight - 120;
        result.Landing!.TotalWeight.Should().BeApproximately(expectedLandingWeight, 1);

        // Landing should also be within envelope
        result.Landing.IsWithinEnvelope.Should().BeTrue();
    }

    [Fact]
    public async Task Calculate_ShouldRecalculateFuelMoment_ForLanding()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 40 }
            },
            FuelBurnGallons = 30 // Burn most of the fuel
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Landing.Should().NotBeNull();

        // Verify the landing moment is calculated correctly
        result.Landing!.TotalMoment.Should().BeLessThan(result.Takeoff.TotalMoment);
    }

    #endregion

    #region Full Calculation Tests - Arm Format

    [Fact]
    public async Task Calculate_ShouldReturnCorrectResults_ArmFormat()
    {
        // Arrange
        var profile = CreateProfile_ArmFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 40 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Should().NotBeNull();

        // Empty: 1500 lbs
        // Front seats: 340 lbs
        // Fuel: 40 * 6 = 240 lbs
        // Total: 2080 lbs
        result.Takeoff.TotalWeight.Should().BeApproximately(2080, 1);

        // Moment calculation with arm format:
        // Empty: 1500 * 38 = 57000
        // Front seats: 340 * 37 = 12580
        // Fuel: 240 * 48 = 11520
        // Total: 81100
        result.Takeoff.TotalMoment.Should().BeApproximately(81100, 10);

        // CG = 81100 / 2080 = 39.0"
        result.Takeoff.CgArm.Should().BeApproximately(39.0, 0.1);

        result.Takeoff.IsWithinEnvelope.Should().BeTrue();
    }

    #endregion

    #region Station Breakdown Tests

    [Fact]
    public async Task Calculate_ShouldReturnCorrectStationBreakdown()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 170 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 30 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.StationBreakdown.Should().HaveCount(3); // Empty + 2 loaded stations

        // Verify empty weight entry
        var emptyEntry = result.StationBreakdown.First(s => s.StationId == "empty");
        emptyEntry.Weight.Should().Be(1663);
        emptyEntry.Arm.Should().Be(40.5);
        emptyEntry.Moment.Should().BeApproximately(67351.5, 1);

        // Verify front seats entry
        var frontSeatsEntry = result.StationBreakdown.First(s => s.StationId == "front-seats");
        frontSeatsEntry.Weight.Should().Be(170);
        frontSeatsEntry.Arm.Should().BeApproximately(37, 0.1);
        frontSeatsEntry.Moment.Should().BeApproximately(6290, 10);

        // Verify fuel entry
        var fuelEntry = result.StationBreakdown.First(s => s.StationId == "fuel");
        fuelEntry.Weight.Should().BeApproximately(180, 1); // 30 gal * 6 lbs/gal
        fuelEntry.Name.Should().Contain("30.0 gal");
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task Calculate_ShouldWarnOnUnknownStationId()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "unknown-station", Weight = 100 },
                new StationLoadDto { StationId = "front-seats", Weight = 170 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("Unknown station ID"));
    }

    [Fact]
    public async Task Calculate_ShouldWarnOnExceededStationMaxWeight()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 500 }, // Exceeds 400 max
                new StationLoadDto { StationId = "fuel", FuelGallons = 30 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("exceeds maximum"));
    }

    [Fact]
    public async Task Calculate_ShouldWarnOnExceededFuelCapacity()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 170 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 60 } // Exceeds 53 gal capacity
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Warnings.Should().Contain(w => w.Contains("exceeds capacity"));
    }

    [Fact]
    public async Task Calculate_ShouldThrowOnMissingEnvelope()
    {
        // Arrange
        var profile = CreateCessna172SProfile_MomentFormat();
        profile.CgEnvelopes.Clear(); // Remove all envelopes
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 170 }
            }
        };

        // Act
        Func<Task> act = async () => await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Calculate_ShouldThrowOnProfileNotFound()
    {
        // Arrange - empty profiles
        SetupMockProfiles();
        var nonExistentProfileId = Guid.NewGuid();

        var request = new WeightBalanceCalculationRequestDto
        {
            LoadedStations = new List<StationLoadDto>()
        };

        // Act
        Func<Task> act = async () => await _service.Calculate(TestUserId, nonExistentProfileId, request);

        // Assert
        await act.Should().ThrowAsync<WeightBalanceProfileNotFoundException>();
    }

    #endregion

    #region CG Envelope Boundary Tests

    [Fact]
    public async Task Calculate_ShouldDetectForwardCgLimit()
    {
        // Arrange - Create profile with narrow CG envelope
        var profile = new WeightBalanceProfile
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            ProfileName = "Test - Forward CG",
            EmptyWeight = 1500,
            EmptyWeightArm = 35.0, // Forward CG
            MaxTakeoffWeight = 2400,
            LoadingGraphFormat = LoadingGraphFormat.Arm,
            LoadingStations = new List<LoadingStation>
            {
                new LoadingStation
                {
                    Id = "front",
                    Name = "Front",
                    MaxWeight = 400,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 30.0 }, // Very forward arm
                    Point2 = new LoadingGraphPoint { Weight = 400, Value = 30.0 }
                }
            },
            CgEnvelopes = new List<CgEnvelope>
            {
                new CgEnvelope
                {
                    Id = "normal",
                    Name = "Normal",
                    Format = CgEnvelopeFormat.Arm,
                    Limits = new List<CgEnvelopePoint>
                    {
                        new CgEnvelopePoint { Weight = 1400, Arm = 38.0 }, // Forward limit at 38"
                        new CgEnvelopePoint { Weight = 2400, Arm = 38.0 },
                        new CgEnvelopePoint { Weight = 2400, Arm = 45.0 },
                        new CgEnvelopePoint { Weight = 1400, Arm = 45.0 }
                    }
                }
            }
        };
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front", Weight = 400 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert - CG should be forward of limits
        // Empty CG: 1500 * 35 = 52500
        // Front: 400 * 30 = 12000
        // Total: 1900 lbs, moment = 64500, CG = 33.9" (forward of 38" limit)
        result.Takeoff.CgArm.Should().BeLessThan(38.0);
        result.Takeoff.IsWithinEnvelope.Should().BeFalse();
    }

    #endregion

    #region Mixed Format Tests (Loading Graph vs Envelope)

    [Fact]
    public async Task Calculate_ShouldWork_WithMomentLoadingGraph_AndArmEnvelope()
    {
        // Arrange - Loading graph uses Moment/1000, but envelope uses Arm
        var profile = new WeightBalanceProfile
        {
            Id = Guid.NewGuid(),
            UserId = TestUserId,
            ProfileName = "Mixed Format Test",
            EmptyWeight = 1500,
            EmptyWeightArm = 40.0,
            MaxTakeoffWeight = 2400,
            LoadingGraphFormat = LoadingGraphFormat.MomentDividedBy1000, // Moment format
            LoadingStations = new List<LoadingStation>
            {
                new LoadingStation
                {
                    Id = "front",
                    Name = "Front Seats",
                    MaxWeight = 400,
                    StationType = LoadingStationType.Standard,
                    Point1 = new LoadingGraphPoint { Weight = 0, Value = 0 },
                    Point2 = new LoadingGraphPoint { Weight = 400, Value = 14.8 } // moment/1000
                }
            },
            CgEnvelopes = new List<CgEnvelope>
            {
                new CgEnvelope
                {
                    Id = "normal",
                    Name = "Normal",
                    Format = CgEnvelopeFormat.Arm, // Arm format for envelope
                    Limits = new List<CgEnvelopePoint>
                    {
                        new CgEnvelopePoint { Weight = 1400, Arm = 35.0 },
                        new CgEnvelopePoint { Weight = 2400, Arm = 35.0 },
                        new CgEnvelopePoint { Weight = 2400, Arm = 47.0 },
                        new CgEnvelopePoint { Weight = 1400, Arm = 47.0 }
                    }
                }
            }
        };
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front", Weight = 340 }
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Should().NotBeNull();
        // Total: 1500 + 340 = 1840 lbs
        result.Takeoff.TotalWeight.Should().BeApproximately(1840, 1);
        // CG should be calculated and checked against arm-based envelope
        result.Takeoff.IsWithinEnvelope.Should().BeTrue();
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public async Task Calculate_RealWorldScenario_SoloPilotFullFuel()
    {
        // Arrange - Common scenario: solo pilot with full fuel
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 180 }, // Solo pilot
                new StationLoadDto { StationId = "baggage-1", Weight = 15 }, // Small bag
                new StationLoadDto { StationId = "fuel", FuelGallons = 53 } // Full fuel
            },
            FuelBurnGallons = 8 // 2-hour flight at 4 gph
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Takeoff.IsWithinEnvelope.Should().BeTrue("Solo pilot with full fuel should be within CG limits");
        result.Landing!.IsWithinEnvelope.Should().BeTrue("Landing should also be within CG limits");
        result.Warnings.Should().NotContain(w => w.Contains("exceeds"));
    }

    [Fact]
    public async Task Calculate_RealWorldScenario_ThreeAdultsWithFuel()
    {
        // Arrange - Realistic scenario: pilot, front passenger, and one rear passenger
        // Note: The C172S has very restrictive aft CG limits - heavy rear loading quickly
        // pushes the CG aft of limits. This is why pilots must carefully calculate W&B!
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 }, // Two adults front (170 lbs each)
                new StationLoadDto { StationId = "rear-seats", Weight = 200 }, // One heavier adult or two kids rear
                new StationLoadDto { StationId = "baggage-1", Weight = 0 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 35 } // 35 gallons
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert
        // Total: 1663 + 340 + 200 + 210 = 2413 lbs (under 2550 max)
        // Moment/1000: 67.35 + 12.58 + 14.6 + 10.08 = 104.61
        // At weight 2413: forward limit ~89.2, aft limit ~104.8
        result.Takeoff.TotalWeight.Should().BeLessThan(2550);
        result.Takeoff.IsWithinEnvelope.Should().BeTrue("Three adults with fuel should be within limits");
    }

    [Fact]
    public async Task Calculate_RealWorldScenario_FourHeavyAdults_ShouldExceedAftLimit()
    {
        // Arrange - This tests a dangerous but common misconception:
        // Four heavy adults in a C172 often exceeds aft CG limits, even with reduced fuel!
        // This test verifies that our calculation correctly identifies this unsafe condition.
        var profile = CreateCessna172SProfile_MomentFormat();
        SetupMockProfiles(profile);

        var request = new WeightBalanceCalculationRequestDto
        {
            EnvelopeId = "normal",
            LoadedStations = new List<StationLoadDto>
            {
                new StationLoadDto { StationId = "front-seats", Weight = 340 }, // Two 170 lb adults front
                new StationLoadDto { StationId = "rear-seats", Weight = 340 }, // Two 170 lb adults rear
                new StationLoadDto { StationId = "baggage-1", Weight = 0 },
                new StationLoadDto { StationId = "fuel", FuelGallons = 30 } // Reduced fuel
            }
        };

        // Act
        var result = await _service.Calculate(TestUserId, profile.Id, request);

        // Assert - This scenario exceeds the aft CG limit
        // The CG is pushed aft by the heavy rear passengers
        result.Takeoff.IsWithinEnvelope.Should().BeFalse(
            "Four heavy adults (170 lbs each) should exceed aft CG limit due to heavy rear loading");
        result.Warnings.Should().Contain(w => w.Contains("outside the envelope"));
    }

    #endregion
}
