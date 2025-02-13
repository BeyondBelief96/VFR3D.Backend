using System.Text.Json.Serialization;
using VFR3D.Infrastructure.Services;

namespace VFR3D.Infrastructure.Dtos.Navlog;


public class MagneticVariationResponseDto   
{
    [JsonPropertyName("result")]
    public MagneticVariationResultDto[]? Result { get; set; }
}