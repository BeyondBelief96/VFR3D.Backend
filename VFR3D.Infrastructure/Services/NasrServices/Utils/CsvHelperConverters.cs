using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;

namespace VFR3D.Infrastructure.Services.NasrServices.Utils
{
    public class OptionalDecimalConverter : DecimalConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var cleanText = text.Trim('"');

            if (decimal.TryParse(cleanText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                return decimalValue;

            if (int.TryParse(cleanText, NumberStyles.Any, CultureInfo.InvariantCulture, out int intValue))
                return Convert.ToDecimal(intValue);

            return null;
        }
    }

    public class OptionalIntConverter : Int32Converter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var cleanText = text.Trim('"');
            return int.TryParse(cleanText, NumberStyles.Any, CultureInfo.InvariantCulture, out int value) ? value : null;
        }
    }

    public class OptionalDateConverter : DateTimeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (DateTime.TryParse(text, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                out DateTime value))
            {
                return DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
