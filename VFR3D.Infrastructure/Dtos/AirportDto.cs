namespace VFR3D.Infrastructure.Dtos
{
    public record AirportDto
    {
        public string SiteNo { get; init; } = string.Empty;
        public string? IcaoId { get; init; }
        public string? ArptId { get; init; }
        public string? ArptName { get; init; }
        public string? City { get; init; }
        public string? StateCode { get; init; }
        public string? StateName { get; init; }
        public decimal? LatDecimal { get; init; }
        public decimal? LongDecimal { get; init; }
        public decimal? Elev { get; init; }
        public string? ChartName { get; init; }
        public string? ArptStatus { get; init; }
        public string? FuelTypes { get; init; }
        public DateTime? LastInspection { get; init; }
        public DateTime? LastInfoResponse { get; init; }
        public string? ContactName { get; init; }
        public string? ContactPhoneNumber { get; init; }
    }
}