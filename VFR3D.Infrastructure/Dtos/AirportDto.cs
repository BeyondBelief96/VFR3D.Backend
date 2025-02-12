namespace VFR3D.Infrastructure.Dtos
{
    public class AirportDto
    {
        public string SiteNo { get; set; } = string.Empty;
        public string? IcaoId { get; set; }
        public string? ArptId { get; set; }
        public string? ArptName { get; set; }
        public string? City { get; set; }
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public decimal? LatDecimal { get; set; }
        public decimal? LongDecimal { get; set; }
        public decimal? Elev { get; set; }
        public string? ChartName { get; set; }
        public string? ArptStatus { get; set; }
        public string? FuelTypes { get; set; }
        public DateTime? LastInspection { get; set; }
        public DateTime? LastInfoResponse { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhoneNumber { get; set; }
    }
}