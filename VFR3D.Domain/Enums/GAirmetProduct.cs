namespace VFR3D.Domain.Enums;

/// <summary>
/// G-AIRMET product types
/// </summary>
public enum GAirmetProduct
{
    /// <summary>
    /// AIRMET Sierra - IFR conditions (ceilings &lt; 1,000 ft, visibility &lt; 3 miles)
    /// and Mountain Obscuration (caused by clouds, precipitation, haze, fog, or smoke)
    /// </summary>
    SIERRA,

    /// <summary>
    /// AIRMET Tango - Moderate turbulence, sustained surface winds of 30 knots or greater,
    /// and non-convective low-level wind shear
    /// </summary>
    TANGO,

    /// <summary>
    /// AIRMET Zulu - Moderate icing and freezing level heights
    /// </summary>
    ZULU
}
