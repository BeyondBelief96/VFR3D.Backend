using VFR3D.Domain.Enums;

namespace VFR3D.Domain.Utilities.UnitConversions;

public static class AirspeedConversion
{
    private const double MphToKnots = 0.868976;
    private const double KphToKnots = 0.539957;
    private const double KnotsToMph = 1.15078;
    private const double KnotsToKph = 1.852;

    /// <summary>
    /// Converts an airspeed value from the specified units to knots.
    /// </summary>
    /// <param name="value">The airspeed value to convert.</param>
    /// <param name="fromUnit">The unit of the input value.</param>
    /// <returns>The airspeed in knots.</returns>
    public static double ToKnots(double value, AirspeedUnits fromUnit)
    {
        return fromUnit switch
        {
            AirspeedUnits.Knots => value,
            AirspeedUnits.MPH => value * MphToKnots,
            AirspeedUnits.KPH => value * KphToKnots,
            _ => throw new ArgumentOutOfRangeException(nameof(fromUnit), fromUnit, "Unknown airspeed unit")
        };
    }

    /// <summary>
    /// Converts an airspeed value from knots to the specified units.
    /// </summary>
    /// <param name="value">The airspeed value in knots.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The airspeed in the target unit.</returns>
    public static double FromKnots(double value, AirspeedUnits toUnit)
    {
        return toUnit switch
        {
            AirspeedUnits.Knots => value,
            AirspeedUnits.MPH => value * KnotsToMph,
            AirspeedUnits.KPH => value * KnotsToKph,
            _ => throw new ArgumentOutOfRangeException(nameof(toUnit), toUnit, "Unknown airspeed unit")
        };
    }

    /// <summary>
    /// Converts an airspeed value from the specified units to knots and rounds to the nearest integer.
    /// </summary>
    /// <param name="value">The airspeed value to convert.</param>
    /// <param name="fromUnit">The unit of the input value.</param>
    /// <returns>The airspeed in knots, rounded to the nearest integer.</returns>
    public static int ToKnotsInt(int value, AirspeedUnits fromUnit)
    {
        return (int)Math.Round(ToKnots(value, fromUnit));
    }

    /// <summary>
    /// Converts an airspeed value from knots to the specified units and rounds to the nearest integer.
    /// </summary>
    /// <param name="value">The airspeed value in knots.</param>
    /// <param name="toUnit">The target unit.</param>
    /// <returns>The airspeed in the target unit, rounded to the nearest integer.</returns>
    public static int FromKnotsInt(int value, AirspeedUnits toUnit)
    {
        return (int)Math.Round(FromKnots(value, toUnit));
    }
}
