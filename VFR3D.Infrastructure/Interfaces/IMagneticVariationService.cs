namespace VFR3D.Infrastructure.Services;

public interface IMagneticVariationService
{
    Task<double> GetMagneticVariation(double latitude, double longitude);
}