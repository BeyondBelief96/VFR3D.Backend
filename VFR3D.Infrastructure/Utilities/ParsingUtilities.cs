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
    }
}
