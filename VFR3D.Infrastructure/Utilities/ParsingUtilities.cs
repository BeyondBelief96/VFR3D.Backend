namespace VFR3D.Infrastructure.Utilities
{
    public static class ParsingUtilities
    {
        public static float? ParseNullableFloat(string? value)
        => float.TryParse(value, out var result) ? result : null;

        public static int? ParseNullableInt(string? value)
            => int.TryParse(value, out var result) ? result : null;

        public static short? ParseNullableShort(string? value)
           => short.TryParse(value, out var result) ? result : null;

        public static float ParseFloat(string? value)
            => float.TryParse(value, out var result) ? result : 0f;

        public static int ParseInt(string? value)
            => int.TryParse(value, out var result) ? result : 0;

        public static double ParseDouble(string? value)
            => double.TryParse(value, out var result) ? result : 0d;
    }
}
