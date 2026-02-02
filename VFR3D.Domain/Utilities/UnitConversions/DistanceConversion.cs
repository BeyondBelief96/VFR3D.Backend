namespace VFR3D.Domain.Utilities.UnitConversions;

public static class DistanceConversion
{
    /// <summary>
    /// The number of meters in one nautical mile.
    /// </summary>
    public const double MetersPerNauticalMile = 1852.0;

    /// <summary>
    /// Converts a distance value from meters to nautical miles.
    /// </summary>
    /// <param name="meters">The distance in meters.</param>
    /// <returns>The distance in nautical miles.</returns>
    public static double ToNauticalMiles(double meters)
    {
        return meters / MetersPerNauticalMile;
    }

    /// <summary>
    /// Converts a distance value from nautical miles to meters.
    /// </summary>
    /// <param name="nauticalMiles">The distance in nautical miles.</param>
    /// <returns>The distance in meters.</returns>
    public static double ToMeters(double nauticalMiles)
    {
        return nauticalMiles * MetersPerNauticalMile;
    }
}
