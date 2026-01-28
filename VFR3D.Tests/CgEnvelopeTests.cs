using FluentAssertions;
using VFR3D.Domain.ValueObjects.WeightBalance;
using Xunit;

namespace VFR3D.Tests;

/// <summary>
/// Tests for CG envelope point-in-polygon detection.
/// These tests verify the ray casting algorithm correctly identifies
/// whether a weight/CG point is inside or outside the envelope polygon.
/// </summary>
public class CgEnvelopeTests
{
    #region HorizontalValue Property Tests

    [Fact]
    public void HorizontalValue_ShouldReturnArm_WhenArmIsSet()
    {
        // Arrange
        var point = new CgEnvelopePoint
        {
            Weight = 2000,
            Arm = 40.5,
            MomentDividedBy1000 = null
        };

        // Act & Assert
        point.HorizontalValue.Should().Be(40.5);
    }

    [Fact]
    public void HorizontalValue_ShouldReturnMomentDividedBy1000_WhenMomentIsSet()
    {
        // Arrange
        var point = new CgEnvelopePoint
        {
            Weight = 2000,
            Arm = null,
            MomentDividedBy1000 = 85.5
        };

        // Act & Assert
        point.HorizontalValue.Should().Be(85.5);
    }

    [Fact]
    public void HorizontalValue_ShouldPreferArm_WhenBothAreSet()
    {
        // Arrange - Arm takes precedence (via null-coalescing order)
        var point = new CgEnvelopePoint
        {
            Weight = 2000,
            Arm = 40.5,
            MomentDividedBy1000 = 85.5
        };

        // Act & Assert
        point.HorizontalValue.Should().Be(40.5);
    }

    [Fact]
    public void HorizontalValue_ShouldReturnZero_WhenNeitherIsSet()
    {
        // Arrange
        var point = new CgEnvelopePoint
        {
            Weight = 2000,
            Arm = null,
            MomentDividedBy1000 = null
        };

        // Act & Assert
        point.HorizontalValue.Should().Be(0);
    }

    #endregion

    #region Point-in-Polygon Algorithm Validation

    /// <summary>
    /// These tests validate the IsPointInEnvelope algorithm indirectly through
    /// the service, but we can also test the envelope shapes directly.
    /// The algorithm uses ray casting to determine if a point is inside a polygon.
    /// </summary>

    [Theory]
    [InlineData(2000, 80, true)]   // Center of envelope
    [InlineData(2000, 57, false)]  // Left of envelope (forward CG)
    [InlineData(2000, 106, false)] // Right of envelope (aft CG)
    [InlineData(2700, 90, false)]  // Above envelope (overweight)
    [InlineData(1400, 60, false)]  // Below envelope (underweight)
    [InlineData(1500, 60, true)]   // Inside at min weight
    [InlineData(2500, 100, true)]  // Inside near top (not on edge)
    public void IsPointInEnvelope_SimpleRectangle_ShouldDetectCorrectly(
        double weight, double momentDiv1000, bool expectedInside)
    {
        // Arrange - Simple rectangular envelope for testing
        // This mimics a simplified CG envelope
        var envelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 58 },
            new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 58 },
            new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 105 },
            new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 105 }
        };

        // Act
        var result = IsPointInPolygon(weight, momentDiv1000, envelope);

        // Assert
        result.Should().Be(expectedInside,
            $"Point ({weight}, {momentDiv1000}) should be {(expectedInside ? "inside" : "outside")} the envelope");
    }

    [Theory]
    [InlineData(2000, 79, true)]   // Center (between forward ~75 and aft ~83 at weight 2000)
    [InlineData(1600, 63, true)]   // Lower section inside (between forward ~60.6 and aft ~67)
    [InlineData(2400, 95, true)]   // Upper section inside (between forward ~89.5 and aft ~99)
    [InlineData(1500, 55, false)]  // Below forward limit at low weight
    [InlineData(2550, 110, false)] // Above aft limit at high weight
    [InlineData(2000, 85, false)]  // Outside aft limit at weight 2000 (aft limit is ~83)
    public void IsPointInEnvelope_TrapezoidShape_ShouldDetectCorrectly(
        double weight, double momentDiv1000, bool expectedInside)
    {
        // Arrange - Trapezoidal envelope (realistic CG envelope shape)
        // Forward limit: (1500,57) -> (1950,73) -> (2550,95)
        // Aft limit: (2550,105) -> (1500,63) - note: diagonal creates narrow aft limit at mid-weights!
        // At weight 2000: forward limit ~75, aft limit ~83
        // At weight 2400: forward limit ~89.5, aft limit ~99
        var envelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 57 },  // Bottom forward
            new CgEnvelopePoint { Weight = 1950, MomentDividedBy1000 = 73 },  // Mid forward
            new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 95 },  // Top forward
            new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 105 }, // Top aft
            new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 63 }   // Bottom aft
        };

        // Act
        var result = IsPointInPolygon(weight, momentDiv1000, envelope);

        // Assert
        result.Should().Be(expectedInside);
    }

    [Fact]
    public void IsPointInEnvelope_ShouldReturnFalse_ForLessThanThreePoints()
    {
        // Arrange - Invalid polygon (need at least 3 points)
        var envelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, MomentDividedBy1000 = 57 },
            new CgEnvelopePoint { Weight = 2550, MomentDividedBy1000 = 95 }
        };

        // Act
        var result = IsPointInPolygon(2000, 80, envelope);

        // Assert
        result.Should().BeFalse("A polygon requires at least 3 points");
    }

    [Fact]
    public void IsPointInEnvelope_ShouldHandleTriangle()
    {
        // Arrange - Triangular envelope
        // Vertices: (1500,35), (2500,40), (2500,45)
        // At weight 2200: forward limit = 38.5, aft limit = 42
        // At weight 1600: forward limit = 35.5, aft limit = 36 (very narrow!)
        var envelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, Arm = 35 },
            new CgEnvelopePoint { Weight = 2500, Arm = 40 },
            new CgEnvelopePoint { Weight = 2500, Arm = 45 }
        };

        // Act & Assert
        IsPointInPolygon(2200, 40, envelope).Should().BeTrue("Point (2200, 40) should be inside triangle");
        IsPointInPolygon(2400, 43, envelope).Should().BeTrue("Point (2400, 43) should be inside triangle");
        IsPointInPolygon(1600, 37, envelope).Should().BeFalse("Point (1600, 37) should be outside triangle (aft of limit)");
        IsPointInPolygon(1600, 34, envelope).Should().BeFalse("Point (1600, 34) should be outside triangle (forward of limit)");
    }

    [Fact]
    public void IsPointInEnvelope_ShouldHandleComplexPolygon()
    {
        // Arrange - More complex shape with multiple vertices
        var envelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, Arm = 35 },
            new CgEnvelopePoint { Weight = 1800, Arm = 36 },
            new CgEnvelopePoint { Weight = 2100, Arm = 37 },
            new CgEnvelopePoint { Weight = 2400, Arm = 38 },
            new CgEnvelopePoint { Weight = 2400, Arm = 47 },
            new CgEnvelopePoint { Weight = 2100, Arm = 47 },
            new CgEnvelopePoint { Weight = 1800, Arm = 46 },
            new CgEnvelopePoint { Weight = 1500, Arm = 45 }
        };

        // Act & Assert
        IsPointInPolygon(2000, 42, envelope).Should().BeTrue("Center point should be inside");
        IsPointInPolygon(2000, 34, envelope).Should().BeFalse("Point forward of limits should be outside");
        IsPointInPolygon(2000, 48, envelope).Should().BeFalse("Point aft of limits should be outside");
    }

    #endregion

    #region Utility Category Envelope Tests

    [Fact]
    public void IsPointInEnvelope_UtilityCategory_ShouldHaveMoreRestrictiveLimits()
    {
        // Arrange - Utility category has more restrictive aft CG limit
        var normalEnvelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, Arm = 35 },
            new CgEnvelopePoint { Weight = 2400, Arm = 35 },
            new CgEnvelopePoint { Weight = 2400, Arm = 47 },
            new CgEnvelopePoint { Weight = 1500, Arm = 47 }
        };

        var utilityEnvelope = new List<CgEnvelopePoint>
        {
            new CgEnvelopePoint { Weight = 1500, Arm = 35 },
            new CgEnvelopePoint { Weight = 2200, Arm = 35 }, // Lower max weight
            new CgEnvelopePoint { Weight = 2200, Arm = 40 }, // More restrictive aft limit
            new CgEnvelopePoint { Weight = 1500, Arm = 40 }
        };

        // Point that's valid for normal but not utility
        double weight = 2300;
        double arm = 42;

        // Act & Assert
        IsPointInPolygon(weight, arm, normalEnvelope).Should().BeTrue("Should be inside normal envelope");
        IsPointInPolygon(weight, arm, utilityEnvelope).Should().BeFalse("Should be outside utility envelope");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Implements the same ray casting algorithm used in WeightBalanceProfileService.
    /// This allows us to test the algorithm directly without database dependencies.
    /// </summary>
    private static bool IsPointInPolygon(double weight, double horizontalValue, List<CgEnvelopePoint> envelope)
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

    #endregion
}
