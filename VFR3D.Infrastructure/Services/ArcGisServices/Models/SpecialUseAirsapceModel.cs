using System.Text.Json.Serialization;

namespace VFR3D.Infrastructure.Services.ArcgisServices.Models
{
    public class SpecialUseAirspaceModel
    {
        [JsonPropertyName("OBJECTID")]
        public int ObjectId { get; set; }

        [JsonPropertyName("GLOBAL_ID")]
        public string? GlobalId { get; set; }

        [JsonPropertyName("NAME")]
        public string? Name { get; set; }

        [JsonPropertyName("TYPE_CODE")]
        public string? TypeCode { get; set; }

        [JsonPropertyName("UPPER_DESC")]
        public string? UpperDesc { get; set; }

        [JsonPropertyName("UPPER_VAL")]
        public string? UpperVal { get; set; }

        [JsonPropertyName("UPPER_UOM")]
        public string? UpperUom { get; set; }

        [JsonPropertyName("UPPER_CODE")]
        public string? UpperCode { get; set; }

        [JsonPropertyName("LOWER_DESC")]
        public string? LowerDesc { get; set; }

        [JsonPropertyName("LOWER_VAL")]
        public string? LowerVal { get; set; }

        [JsonPropertyName("LOWER_UOM")]
        public string? LowerUom { get; set; }

        [JsonPropertyName("LOWER_CODE")]
        public string? LowerCode { get; set; }

        [JsonPropertyName("TIMESOFUSE")]
        public string? TimesOfUse { get; set; }

        [JsonPropertyName("REMARKS")]
        public string? Remarks { get; set; }

        [JsonPropertyName("CLASS")]
        public string? Class { get; set; }

        [JsonPropertyName("SECTOR")]
        public string? Sector { get; set; }

        [JsonPropertyName("ONSHORE")]
        public string? Onshore { get; set; }

        [JsonPropertyName("EXCLUSION")]
        public string? Exclusion { get; set; }

        [JsonPropertyName("GMTOFFSET")]
        public string? GmtOffset { get; set; }

        [JsonPropertyName("CONT_AGENT")]
        public string? ContAgent { get; set; }

        [JsonPropertyName("CITY")]
        public string? City { get; set; }

        [JsonPropertyName("STATE")]
        public string? State { get; set; }

        [JsonPropertyName("COUNTRY")]
        public string? Country { get; set; }

        [JsonPropertyName("US_HIGH")]
        public short? UsHigh { get; set; }

        [JsonPropertyName("AK_HIGH")]
        public short? AkHigh { get; set; }

        [JsonPropertyName("AK_LOW")]
        public short? AkLow { get; set; }

        [JsonPropertyName("US_LOW")]
        public short? UsLow { get; set; }

        [JsonPropertyName("US_AREA")]
        public short? UsArea { get; set; }

        [JsonPropertyName("PACIFIC")]
        public short? Pacific { get; set; }

        [JsonPropertyName("Shape__Area")]
        public float? ShapeArea { get; set; }

        [JsonPropertyName("Shape__Length")]
        public float? ShapeLength { get; set; }
    }
}