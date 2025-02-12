namespace VFR3D.Infrastructure.Dtos;

public record CommunicationFrequencyDto()
{
    public Guid Id { get; init; }
    public string? FacilityCode { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string? FacilityName { get; init; }
    public string FacilityType { get; init; } = string.Empty;
    public string? ArtccOrFssId { get; init; }
    public string? Cpdlc { get; init; }
    public string? TowerHours { get; init; }
    public string ServicedFacility { get; init; } = string.Empty;
    public string? ServicedFacilityName { get; init; }
    public string? ServicedSiteType { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? ServicedCity { get; init; }
    public string? ServicedState { get; init; }
    public string? ServicedCountry { get; init; }
    public string? TowerOrCommCall { get; init; }
    public string? PrimaryApproachRadioCall { get; init; }
    public string? Frequency { get; init; }
    public string? Sectorization { get; init; }
    public string? FrequencyUse { get; init; }
    public string? Remark { get; init; }
}