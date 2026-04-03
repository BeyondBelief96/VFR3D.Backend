using System.Text.Json.Serialization;

namespace VFR3D.Infrastructure.Dtos.Notam;

/// <summary>
/// Represents a NOTAM as a GeoJSON Feature from the NMS API.
/// See nms-api.yaml NmsNotamData schema for full specification.
/// </summary>
public record NotamDto
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "Feature";

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("geometry")]
    public NotamGeometryDto? Geometry { get; init; }

    [JsonPropertyName("properties")]
    public NotamPropertiesDto? Properties { get; init; }
}

/// <summary>
/// GeoJSON geometry - can be Point, Polygon, or GeometryCollection
/// </summary>
public record NotamGeometryDto
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// For Point: [lon, lat] array
    /// For Polygon: array of coordinate rings
    /// For GeometryCollection: null (use Geometries instead)
    /// </summary>
    [JsonPropertyName("coordinates")]
    public object? Coordinates { get; init; }

    /// <summary>
    /// For GeometryCollection type - contains child geometries
    /// </summary>
    [JsonPropertyName("geometries")]
    public List<NotamGeometryDto>? Geometries { get; init; }
}

public record NotamPropertiesDto
{
    [JsonPropertyName("coreNOTAMData")]
    public CoreNotamDataDto? CoreNotamData { get; init; }
}

public record CoreNotamDataDto
{
    [JsonPropertyName("notamEvent")]
    public NotamEventDto? NotamEvent { get; init; }

    [JsonPropertyName("notam")]
    public NotamDetailDto? Notam { get; init; }

    [JsonPropertyName("notamTranslation")]
    public List<NotamTranslationDto>? NotamTranslation { get; init; }
}

/// <summary>
/// NOTAM event metadata
/// </summary>
public record NotamEventDto
{
    [JsonPropertyName("encoding")]
    public string? Encoding { get; init; }

    [JsonPropertyName("scenario")]
    public string? Scenario { get; init; }
}

/// <summary>
/// Core NOTAM detail fields per NMS API specification.
/// Classification values: INTERNATIONAL, MILITARY, LOCAL_MILITARY, DOMESTIC, FDC
/// Feature values: RWY, TWY, APRON, AD, OBST, NAV, COM, SVC, AIRSPACE, ODP, SID, STAR, CHART, DATA, DVA, IAP, VFP, ROUTE, SPECIAL, SECURITY
/// Series values: A, B, C, D, E, G, H, I, J, K, N, R, V, Z
/// </summary>
public record NotamDetailDto
{
    /// <summary>
    /// Unique 16-digit NMS identifier
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// NOTAM number (e.g., "01/123", "A1234/25")
    /// </summary>
    [JsonPropertyName("number")]
    public string? Number { get; init; }

    /// <summary>
    /// ICAO series code (A, B, C, D, E, G, H, I, J, K, N, R, V, Z)
    /// </summary>
    [JsonPropertyName("series")]
    public string? Series { get; init; }

    /// <summary>
    /// NOTAM year
    /// </summary>
    [JsonPropertyName("year")]
    public string? Year { get; init; }

    /// <summary>
    /// NOTAM type (N=New, R=Replace, C=Cancel)
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Issuance timestamp (ISO 8601)
    /// </summary>
    [JsonPropertyName("issued")]
    public string? Issued { get; init; }

    /// <summary>
    /// Affected FIR/ARTCC (e.g., "ZTL")
    /// </summary>
    [JsonPropertyName("affectedFir")]
    public string? AffectedFir { get; init; }

    /// <summary>
    /// Q-code selection code (e.g., "QXXX")
    /// </summary>
    [JsonPropertyName("selectionCode")]
    public string? SelectionCode { get; init; }

    /// <summary>
    /// Traffic type (I=IFR, V=VFR, IV=Both)
    /// </summary>
    [JsonPropertyName("traffic")]
    public string? Traffic { get; init; }

    /// <summary>
    /// Purpose code (e.g., "BO")
    /// </summary>
    [JsonPropertyName("purpose")]
    public string? Purpose { get; init; }

    /// <summary>
    /// Scope (A=Aerodrome, E=En-route, W=Navigation warning)
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    /// <summary>
    /// Minimum flight level (e.g., "000")
    /// </summary>
    [JsonPropertyName("minimumFl")]
    public string? MinimumFl { get; init; }

    /// <summary>
    /// Maximum flight level (e.g., "999")
    /// </summary>
    [JsonPropertyName("maximumFl")]
    public string? MaximumFl { get; init; }

    /// <summary>
    /// Domestic location identifier (e.g., "CLT")
    /// </summary>
    [JsonPropertyName("location")]
    public string? Location { get; init; }

    /// <summary>
    /// ICAO location identifier (e.g., "KCLT")
    /// </summary>
    [JsonPropertyName("icaoLocation")]
    public string? IcaoLocation { get; init; }

    /// <summary>
    /// Effective start timestamp (ISO 8601)
    /// </summary>
    [JsonPropertyName("effectiveStart")]
    public string? EffectiveStart { get; init; }

    /// <summary>
    /// Effective end timestamp (ISO 8601)
    /// </summary>
    [JsonPropertyName("effectiveEnd")]
    public string? EffectiveEnd { get; init; }

    /// <summary>
    /// Whether end time is estimated ("true"/"false")
    /// </summary>
    [JsonPropertyName("estimated")]
    public string? Estimated { get; init; }

    /// <summary>
    /// NOTAM text content
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    /// Classification: INTERNATIONAL, MILITARY, LOCAL_MILITARY, DOMESTIC, FDC
    /// </summary>
    [JsonPropertyName("classification")]
    public string? Classification { get; init; }

    /// <summary>
    /// Cancellation date timestamp (ISO 8601)
    /// </summary>
    [JsonPropertyName("cancelationDate")]
    public string? CancelationDate { get; init; }

    /// <summary>
    /// Accountability ID (3-4 char domestic or 8 char AFTN)
    /// </summary>
    [JsonPropertyName("accountId")]
    public string? AccountId { get; init; }

    /// <summary>
    /// Last updated timestamp (ISO 8601)
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public string? LastUpdated { get; init; }

    /// <summary>
    /// Schedule string (e.g., "Daily:1200-1230~DLY 1200-1230")
    /// </summary>
    [JsonPropertyName("schedule")]
    public string? Schedule { get; init; }

    /// <summary>
    /// Lower altitude limit (e.g., "SFC", "3000FT")
    /// </summary>
    [JsonPropertyName("lowerLimit")]
    public string? LowerLimit { get; init; }

    /// <summary>
    /// Upper altitude limit (e.g., "280M", "FL180")
    /// </summary>
    [JsonPropertyName("upperLimit")]
    public string? UpperLimit { get; init; }

    /// <summary>
    /// ICAO coordinates string (e.g., "3939N04302E")
    /// </summary>
    [JsonPropertyName("coordinates")]
    public string? Coordinates { get; init; }

    /// <summary>
    /// Radius in nautical miles
    /// </summary>
    [JsonPropertyName("radius")]
    public string? Radius { get; init; }
}

/// <summary>
/// NOTAM translation in various formats.
/// Type values: LOCAL_FORMAT, ICAO
/// </summary>
public record NotamTranslationDto
{
    /// <summary>
    /// Translation type: LOCAL_FORMAT or ICAO
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>
    /// Simple text translation (plain English)
    /// </summary>
    [JsonPropertyName("simpleText")]
    public string? SimpleText { get; init; }

    /// <summary>
    /// Domestic format message (for LOCAL_FORMAT type)
    /// </summary>
    [JsonPropertyName("domestic_message")]
    public string? DomesticMessage { get; init; }

    /// <summary>
    /// ICAO format message (for ICAO type)
    /// </summary>
    [JsonPropertyName("icao_message")]
    public string? IcaoMessage { get; init; }

    /// <summary>
    /// Formatted text (HTML or rich text)
    /// </summary>
    [JsonPropertyName("formattedText")]
    public string? FormattedText { get; init; }
}
