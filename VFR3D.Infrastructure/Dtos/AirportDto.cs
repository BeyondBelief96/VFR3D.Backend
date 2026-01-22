namespace VFR3D.Infrastructure.Dtos
{
    public record AirportDto
    {
        public string SiteNo { get; init; } = string.Empty;
        public string? IcaoId { get; init; }
        public string? ArptId { get; init; }
        public string? ArptName { get; init; }
        public string? SiteTypeCode { get; init; }
        public string? City { get; init; }
        public string? StateCode { get; init; }
        public string? CountryCode { get; init; }
        public string? StateName { get; init; }
        public decimal? LatDecimal { get; init; }
        public decimal? LongDecimal { get; init; }
        public decimal? Elev { get; init; }
        public decimal? MagVarn { get; init; }
        public string? MagHemis { get; init; }
        public string? ChartName { get; init; }
        public string? ArptStatus { get; init; }
        public string? FuelTypes { get; init; }
        public DateTime? LastInspection { get; init; }
        public DateTime? LastInfoResponse { get; init; }
        
        public string? CustomsFlag { get; init; }
        
        public string? LndgRightsFlag { get; init; }
        
        public string? JointUseFlag { get; init; }
        
        public string? MilLndgFlag { get; init; }
        public string? ContactTitle { get; init; }
        public string? ContactName { get; init; }
        
        public string? ContactAddress1 { get; init; }
        
        public string? ContactAddress2 { get; init; }
        
        public string? ContactCity { get; init; }
        
        public string? ContactState { get; init; }
        
        public string? ContactZipCode { get; init; }
        
        public string? ContactZipPlusFour { get; init; }
        public string? ContactPhoneNumber { get; init; }
    }
}