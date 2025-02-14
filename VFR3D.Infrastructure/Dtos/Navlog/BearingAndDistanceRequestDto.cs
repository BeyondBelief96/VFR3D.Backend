namespace VFR3D.Infrastructure.Dtos.Navlog;

public record BearingAndDistanceRequestDto
{
    public double StartLatitude { get; init; }
    
    public double StartLongitude { get; init; }
    
    public double EndLatitude { get; init; }
    
    public double EndLongitude { get; init; }
}