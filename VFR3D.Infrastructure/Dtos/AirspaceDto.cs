using System.Text.Json.Serialization;

namespace VFR3D.Infrastructure.Dtos
{
    public class GeoJsonGeometry
    {
        public string Type { get; set; } = string.Empty;
        public double[][][] Coordinates { get; set; } = [];
    }

    public class AirspaceDto
    {
        public string? GlobalId { get; set; }
        public string? Ident { get; set; }
        public string? IcaoId { get; set; }
        public string? Name { get; set; }
        public string? UpperDesc { get; set; }
        public double? UpperVal { get; set; }
        public string? UpperUom { get; set; }
        public string? UpperCode { get; set; }
        public string? LowerDesc { get; set; }
        public double? LowerVal { get; set; }
        public string? LowerUom { get; set; }
        public string? LowerCode { get; set; }
        public string? TypeCode { get; set; }
        public string? LocalType { get; set; }
        public string? Class { get; set; }
        public string? MilCode { get; set; }
        public string? CommName { get; set; }
        public string? Level { get; set; }
        public string? Sector { get; set; }
        public string? Onshore { get; set; }
        public string? Exclusion { get; set; }
        public string? WkhrCode { get; set; }
        public string? WkhrRmk { get; set; }
        public string? Dst { get; set; }
        public string? GmtOffset { get; set; }
        public string? ContAgent { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? AdhpId { get; set; }
        public GeoJsonGeometry? Geometry { get; set; }
    }

    public class SpecialUseAirspaceDto
    {
        public string? GlobalId { get; set; }
        public string? Name { get; set; }
        public string? TypeCode { get; set; }
        public string? Class { get; set; }
        public string? UpperDesc { get; set; }
        public string? UpperVal { get; set; }
        public string? UpperUom { get; set; }
        public string? UpperCode { get; set; }
        public string? LowerDesc { get; set; }
        public string? LowerVal { get; set; }
        public string? LowerUom { get; set; }
        public string? LowerCode { get; set; }
        public string? LevelCode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ContAgent { get; set; }
        public string? CommName { get; set; }
        public string? Sector { get; set; }
        public string? Onshore { get; set; }
        public string? Exclusion { get; set; }
        public string? TimesOfUse { get; set; }
        public string? GmtOffset { get; set; }
        public string? DstCode { get; set; }
        public string? Remarks { get; set; }
        public GeoJsonGeometry? Geometry { get; set; }
    }
}