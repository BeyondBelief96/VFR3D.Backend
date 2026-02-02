using FluentAssertions;
using VFR3D.Domain.Enums;
using VFR3D.Domain.Utilities.UnitConversions;
using Xunit;

namespace VFR3D.Tests.UnitConversionTests;

public class AirspeedConversionTests
{
    [Theory]
    [InlineData(100, AirspeedUnits.Knots, 100)]
    [InlineData(100, AirspeedUnits.MPH, 86.8976)]
    [InlineData(100, AirspeedUnits.KPH, 53.9957)]
    public void ToKnots_ShouldConvertCorrectly(double value, AirspeedUnits fromUnit, double expectedKnots)
    {
        // Act
        var result = AirspeedConversion.ToKnots(value, fromUnit);

        // Assert
        result.Should().BeApproximately(expectedKnots, 0.01);
    }

    [Theory]
    [InlineData(100, AirspeedUnits.Knots, 100)]
    [InlineData(100, AirspeedUnits.MPH, 115.078)]
    [InlineData(100, AirspeedUnits.KPH, 185.2)]
    public void FromKnots_ShouldConvertCorrectly(double knotsValue, AirspeedUnits toUnit, double expectedValue)
    {
        // Act
        var result = AirspeedConversion.FromKnots(knotsValue, toUnit);

        // Assert
        result.Should().BeApproximately(expectedValue, 0.01);
    }

    [Fact]
    public void ToKnots_FromMPH_AndBack_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalMph = 150.0;

        // Act
        var knots = AirspeedConversion.ToKnots(originalMph, AirspeedUnits.MPH);
        var backToMph = AirspeedConversion.FromKnots(knots, AirspeedUnits.MPH);

        // Assert
        backToMph.Should().BeApproximately(originalMph, 0.01);
    }

    [Fact]
    public void ToKnots_FromKPH_AndBack_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalKph = 200.0;

        // Act
        var knots = AirspeedConversion.ToKnots(originalKph, AirspeedUnits.KPH);
        var backToKph = AirspeedConversion.FromKnots(knots, AirspeedUnits.KPH);

        // Assert
        backToKph.Should().BeApproximately(originalKph, 0.01);
    }

    [Theory]
    [InlineData(115, AirspeedUnits.MPH, 100)] // 115 MPH ≈ 100 knots
    [InlineData(185, AirspeedUnits.KPH, 100)] // 185 KPH ≈ 100 knots
    [InlineData(120, AirspeedUnits.Knots, 120)]
    public void ToKnotsInt_ShouldRoundCorrectly(int value, AirspeedUnits fromUnit, int expectedKnots)
    {
        // Act
        var result = AirspeedConversion.ToKnotsInt(value, fromUnit);

        // Assert
        result.Should().Be(expectedKnots);
    }

    [Theory]
    [InlineData(100, AirspeedUnits.MPH, 115)] // 100 knots ≈ 115 MPH
    [InlineData(100, AirspeedUnits.KPH, 185)] // 100 knots ≈ 185 KPH
    [InlineData(120, AirspeedUnits.Knots, 120)]
    public void FromKnotsInt_ShouldRoundCorrectly(int knotsValue, AirspeedUnits toUnit, int expectedValue)
    {
        // Act
        var result = AirspeedConversion.FromKnotsInt(knotsValue, toUnit);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void ToKnots_WithZeroValue_ShouldReturnZero()
    {
        // Act & Assert
        AirspeedConversion.ToKnots(0, AirspeedUnits.MPH).Should().Be(0);
        AirspeedConversion.ToKnots(0, AirspeedUnits.KPH).Should().Be(0);
        AirspeedConversion.ToKnots(0, AirspeedUnits.Knots).Should().Be(0);
    }

    [Fact]
    public void FromKnots_WithZeroValue_ShouldReturnZero()
    {
        // Act & Assert
        AirspeedConversion.FromKnots(0, AirspeedUnits.MPH).Should().Be(0);
        AirspeedConversion.FromKnots(0, AirspeedUnits.KPH).Should().Be(0);
        AirspeedConversion.FromKnots(0, AirspeedUnits.Knots).Should().Be(0);
    }
}
