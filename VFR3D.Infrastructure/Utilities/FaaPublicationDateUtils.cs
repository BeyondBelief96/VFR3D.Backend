using System.Globalization;

namespace VFR3D.Infrastructure.Utilities
{
    public static class FaaPublicationDateUtils
    {
        public static DateTime CalculateCurrentPublicationDate(DateTime knownValidDate, int cycleLengthDays)
        {
            var currentDate = DateTime.UtcNow;
            var daysSinceKnown = (currentDate - knownValidDate).TotalDays;
            var completeCycles = Math.Floor(daysSinceKnown / cycleLengthDays);
            return knownValidDate.AddDays(completeCycles * cycleLengthDays);
        }

        public static string FormatDateForNasr(DateTime date)
        {
            return $"{date.Day:D2}_{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(date.ToString("MMM", CultureInfo.InvariantCulture).ToLower())}_{date.Year}";
        }

        public static string FormatDateForAirportDiagrams(DateTime date)
        {
            return date.ToString("yyMMdd");
        }

        public static string FormatDateForChartSupplements(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }
    }
}
