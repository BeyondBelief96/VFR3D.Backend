using System.Text.Json.Serialization;

namespace VFR3D.Infrastructure.Services.ArcgisServices.Models
{
    public class AirspaceModel
    {
        [JsonPropertyName("OBJECTID")]
        public int ObjectId { get; set; }

        [JsonPropertyName("GLOBAL_ID")]
        public string? GlobalId { get; set; }

        [JsonPropertyName("IDENT")]
        public string? Ident { get; set; }

        [JsonPropertyName("ICAO_ID")]
        public string? IcaoId { get; set; }

        [JsonPropertyName("NAME")]
        public string? Name { get; set; }

        [JsonPropertyName("TYPE_CODE")]
        public string? TypeCode { get; set; }

        [JsonPropertyName("LOCAL_TYPE")]
        public string? LocalType { get; set; }

        [JsonPropertyName("CLASS")]
        public string? Class { get; set; }

        [JsonPropertyName("MIL_CODE")]
        public string? MilCode { get; set; }

        [JsonPropertyName("UPPER_DESC")]
        public string? UpperDesc { get; set; }

        [JsonPropertyName("UPPER_VAL")]
        public double? UpperVal { get; set; }

        [JsonPropertyName("UPPER_UOM")]
        public string? UpperUom { get; set; }

        [JsonPropertyName("UPPER_CODE")]
        public string? UpperCode { get; set; }

        [JsonPropertyName("LOWER_DESC")]
        public string? LowerDesc { get; set; }

        [JsonPropertyName("LOWER_VAL")]
        public double? LowerVal { get; set; }

        [JsonPropertyName("LOWER_UOM")]
        public string? LowerUom { get; set; }

        [JsonPropertyName("LOWER_CODE")]
        public string? LowerCode { get; set; }

        [JsonPropertyName("LEVEL_")]
        public string? Level { get; set; }

        [JsonPropertyName("SECTOR")]
        public string? Sector { get; set; }

        [JsonPropertyName("ONSHORE")]
        public string? Onshore { get; set; }

        [JsonPropertyName("EXCLUSION")]
        public string? Exclusion { get; set; }

        [JsonPropertyName("WKHR_CODE")]
        public string? WkhrCode { get; set; }

        [JsonPropertyName("WKHR_RMK")]
        public string? WkhrRmk { get; set; }

        [JsonPropertyName("DST")]
        public string? Dst { get; set; }

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

        [JsonPropertyName("ADHP_ID")]
        public string? AdhpId { get; set; }

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
        public double? ShapeArea { get; set; }

        [JsonPropertyName("Shape__Length")]
        public double? ShapeLength { get; set; }
    }
}