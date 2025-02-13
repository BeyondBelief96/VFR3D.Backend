using System.Text.Json.Serialization;

namespace VFR3D.Infrastructure.Dtos.Navlog;

public class MagneticVariationResultDto
{
    [JsonPropertyName("date")]
    public float Date { get; set; }

    [JsonPropertyName("elevation")]
    public float Elevation { get; set; }

    [JsonPropertyName("declination")]
    public double Declination { get; set; }

    [JsonPropertyName("latitude")]
    public float Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public float Longitude { get; set; }

    [JsonPropertyName("declnation_sv")]
    public float DeclinationSv { get; set; }

    [JsonPropertyName("declination_uncertainty")]
    public float DeclinationUncertainty { get; set; }
}