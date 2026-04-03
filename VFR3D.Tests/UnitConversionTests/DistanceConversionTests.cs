using FluentAssertions;
using VFR3D.Domain.Utilities.UnitConversions;
using Xunit;

namespace VFR3D.Tests.UnitConversionTests;

public class DistanceConversionTests
{
    [Fact]
    public void MetersPerNauticalMile_ShouldBeCorrectValue()
    {
        // Assert
        DistanceConversion.MetersPerNauticalMile.Should().Be(1852.0);
    }

    [Theory]
    [InlineData(1852, 1)]
    [InlineData(3704, 2)]
    [InlineData(9260, 5)]
    [InlineData(0, 0)]
    public void ToNauticalMiles_ShouldConvertCorrectly(double meters, double expectedNm)
    {
        // Act
        var result = DistanceConversion.ToNauticalMiles(meters);

        // Assert
        result.Should().BeApproximately(expectedNm, 0.001);
    }

    [Theory]
    [InlineData(1, 1852)]
    [InlineData(2, 3704)]
    [InlineData(5, 9260)]
    [InlineData(0, 0)]
    public void ToMeters_ShouldConvertCorrectly(double nm, double expectedMeters)
    {
        // Act
        var result = DistanceConversion.ToMeters(nm);

        // Assert
        result.Should().BeApproximately(expectedMeters, 0.001);
    }

    [Fact]
    public void ToNauticalMiles_AndBack_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalMeters = 10000.0;

        // Act
        var nm = DistanceConversion.ToNauticalMiles(originalMeters);
        var backToMeters = DistanceConversion.ToMeters(nm);

        // Assert
        backToMeters.Should().BeApproximately(originalMeters, 0.001);
    }

    [Fact]
    public void ToMeters_AndBack_ShouldReturnOriginalValue()
    {
        // Arrange
        var originalNm = 100.0;

        // Act
        var meters = DistanceConversion.ToMeters(originalNm);
        var backToNm = DistanceConversion.ToNauticalMiles(meters);

        // Assert
        backToNm.Should().BeApproximately(originalNm, 0.001);
    }

    [Fact]
    public void ToNauticalMiles_CommonAviatonDistances_ShouldConvertCorrectly()
    {
        // KBNA to KATL is approximately 186 NM (344472 meters)
        var bnaToAtlMeters = 186 * 1852; // 344472
        DistanceConversion.ToNauticalMiles(bnaToAtlMeters).Should().BeApproximately(186, 0.01);
    }
}
