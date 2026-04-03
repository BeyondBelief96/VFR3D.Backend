namespace VFR3D.Domain.Enums;

/// <summary>
/// G-AIRMET hazard types
/// </summary>
public enum GAirmetHazardType
{
    // SIERRA hazards (IFR/Mountain Obscuration)

    /// <summary>
    /// Mountain obscuration
    /// </summary>
    MT_OBSC,

    /// <summary>
    /// IFR conditions (ceilings below 1,000 ft, visibility below 3 miles)
    /// </summary>
    IFR,

    // TANGO hazards (Turbulence/Winds)

    /// <summary>
    /// Low-level turbulence (below FL180)
    /// </summary>
    TURB_LO,

    /// <summary>
    /// High-level turbulence (at or above FL180)
    /// </summary>
    TURB_HI,

    /// <summary>
    /// Low-level wind shear
    /// </summary>
    LLWS,

    /// <summary>
    /// Strong surface winds (30 knots or greater)
    /// </summary>
    SFC_WIND,

    // ZULU hazards (Icing/Freezing levels)

    /// <summary>
    /// Moderate icing
    /// </summary>
    ICE,

    /// <summary>
    /// Freezing level
    /// </summary>
    FZLVL,

    /// <summary>
    /// Multiple freezing levels
    /// </summary>
    M_FZLVL
}
