using FluentAssertions;
using VFR3D.Domain.Enums;
using VFR3D.Domain.Utilities.UnitConversions;
using Xunit;

namespace VFR3D.Tests.UnitConversionTests;

public class LengthConversionTests
{
    [Theory]
    [InlineData(100, LengthUnits.Feet, 100)]
    [InlineData(100, LengthUnits.Meters, 328.084)]
    public void ToFeet_ShouldConvertCorrectly(double value, LengthUnits fromUnit, double expectedFeet)
    {
        // Act
        var result = LengthConversion.ToFeet(value, fromUnit);

        // Assert
        result.Should().BeApproximately(expectedFeet, 0.01);
    }

    [Theory]
    [InlineData(100, LengthUnits.Feet, 100)]
    [InlineData(100, LengthUnits.Meters, 30.48)]
    public void FromFeet_ShouldConvertCorrectly(double feetValue, LengthUnits toUnit, double expectedValue)
    {
        // Act
        var result = LengthConversion.FromFeet(feetValue, toUnit);

        // Assert
        result.Should().BeApproximately(expectedValue, 0.01);
    }

    [Fact]
    public void ToFeet_FromMeters_AndBack_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalMeters = 1000.0;

        // Act
        var feet = LengthConversion.ToFeet(originalMeters, LengthUnits.Meters);
        var backToMeters = LengthConversion.FromFeet(feet, LengthUnits.Meters);

        // Assert
        backToMeters.Should().BeApproximately(originalMeters, 0.01);
    }

    [Theory]
    [InlineData(1000, LengthUnits.Meters, 3281)] // 1000 meters ≈ 3281 feet
    [InlineData(5000, LengthUnits.Feet, 5000)]
    public void ToFeetInt_ShouldRoundCorrectly(int value, LengthUnits fromUnit, int expectedFeet)
    {
        // Act
        var result = LengthConversion.ToFeetInt(value, fromUnit);

        // Assert
        result.Should().Be(expectedFeet);
    }

    [Theory]
    [InlineData(1000, LengthUnits.Meters, 305)] // 1000 feet ≈ 305 meters
    [InlineData(5000, LengthUnits.Feet, 5000)]
    public void FromFeetInt_ShouldRoundCorrectly(int feetValue, LengthUnits toUnit, int expectedValue)
    {
        // Act
        var result = LengthConversion.FromFeetInt(feetValue, toUnit);

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void ToFeet_WithZeroValue_ShouldReturnZero()
    {
        // Act & Assert
        LengthConversion.ToFeet(0, LengthUnits.Meters).Should().Be(0);
        LengthConversion.ToFeet(0, LengthUnits.Feet).Should().Be(0);
    }

    [Fact]
    public void FromFeet_WithZeroValue_ShouldReturnZero()
    {
        // Act & Assert
        LengthConversion.FromFeet(0, LengthUnits.Meters).Should().Be(0);
        LengthConversion.FromFeet(0, LengthUnits.Feet).Should().Be(0);
    }

    [Fact]
    public void ToFeet_StandardAviationAltitudes_ShouldConvertCorrectly()
    {
        // Common aviation altitudes in meters converted to feet

        // 3000 meters ≈ 9843 feet (FL100)
        LengthConversion.ToFeet(3000, LengthUnits.Meters).Should().BeApproximately(9842.52, 0.1);

        // 10000 meters ≈ 32808 feet (cruise altitude)
        LengthConversion.ToFeet(10000, LengthUnits.Meters).Should().BeApproximately(32808.4, 0.1);
    }
}
