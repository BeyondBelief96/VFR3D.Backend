namespace VFR3D.Domain.Enums;

/// <summary>
/// AIRSIGMET hazard types
/// </summary>
public enum AirsigmetHazardType
{
    /// <summary>
    /// Convective activity (thunderstorms)
    /// </summary>
    CONVECTIVE,

    /// <summary>
    /// Icing conditions
    /// </summary>
    ICE,

    /// <summary>
    /// Turbulence
    /// </summary>
    TURB,

    /// <summary>
    /// Instrument Flight Rules conditions (low visibility/ceiling)
    /// </summary>
    IFR,

    /// <summary>
    /// Mountain obscuration
    /// </summary>
    MTN_OBSCN
}
