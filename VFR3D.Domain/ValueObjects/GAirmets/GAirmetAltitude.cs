namespace VFR3D.Domain.ValueObjects.GAirmets;

/// <summary>
/// Represents altitude information for a G-AIRMET
/// </summary>
public class GAirmetAltitude
{
    /// <summary>
    /// Minimum altitude in feet MSL. Can be a number, "SFC" for surface, or "FZL" for freezing level
    /// </summary>
    public string? MinFtMsl { get; set; }

    /// <summary>
    /// Maximum altitude in feet MSL
    /// </summary>
    public string? MaxFtMsl { get; set; }

    /// <summary>
    /// Single level altitude in feet MSL (used for freezing level lines)
    /// </summary>
    public string? LevelFtMsl { get; set; }

    /// <summary>
    /// Freezing level altitude range (when min_ft_msl is "FZL")
    /// </summary>
    public GAirmetFzlAltitude? FzlAltitude { get; set; }
}

/// <summary>
/// Represents freezing level altitude range
/// </summary>
public class GAirmetFzlAltitude
{
    /// <summary>
    /// Minimum freezing level in feet MSL
    /// </summary>
    public string? MinFtMsl { get; set; }

    /// <summary>
    /// Maximum freezing level in feet MSL
    /// </summary>
    public string? MaxFtMsl { get; set; }
}
