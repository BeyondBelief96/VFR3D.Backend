using VFR3D.Domain.Enums;

namespace VFR3D.Domain.Utilities.UnitConversions;

public static class LengthConversion
{
    private const double MetersToFeet = 3.28084;
    private const double FeetToMeters = 0.3048;

    /// <summary>
    /// Converts a length value from the specified units to feet.
    /// </summary>
    /// <param name="value">The length value to convert.</param>
    /// <param name="fromUnit">The unit of the input value.</param>
    /// <returns>The length in feet.</returns>
    public static double ToFeet(double value, LengthUnits fromUnit)
    {
        return fromUnit switch
        {
            LengthUnits.Feet => value,
            LengthUnits.Meters => value * MetersToFeet,
            _ => throw new ArgumentOutOfRangeException(nameof(fromUnit), fromUnit, "Unknown length unit")
        };
    }

    /// <summary>
    /// Converts a length value from feet to the specified units.
    /// </summary>
    /// <param name="value">The length value in feet.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The length in the target unit.</returns>
    public static double FromFeet(double value, LengthUnits toUnit)
    {
        return toUnit switch
        {
            LengthUnits.Feet => value,
            LengthUnits.Meters => value * FeetToMeters,
            _ => throw new ArgumentOutOfRangeException(nameof(toUnit), toUnit, "Unknown length unit")
        };
    }

    /// <summary>
    /// Converts a length value from the specified units to feet and rounds to the nearest integer.
    /// </summary>
    /// <param name="value">The length value to convert.</param>
    /// <param name="fromUnit">The unit of the input value.</param>
    /// <returns>The length in feet, rounded to the nearest integer.</returns>
    public static int ToFeetInt(int value, LengthUnits fromUnit)
    {
        return (int)Math.Round(ToFeet(value, fromUnit));
    }

    /// <summary>
    /// Converts a length value from feet to the specified units and rounds to the nearest integer.
    /// </summary>
    /// <param name="value">The length value in feet.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The length in the target unit, rounded to the nearest integer.</returns>
    public static int FromFeetInt(int value, LengthUnits toUnit)
    {
        return (int)Math.Round(FromFeet(value, toUnit));
    }
}
