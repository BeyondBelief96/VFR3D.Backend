using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace VFR3D.Domain.Entities;

[Table("obstacles")]
public class Obstacle
{
    [Key]
    [Column("oas_number", TypeName = "varchar(10)")]
    public string OasNumber { get; set; } = string.Empty;

    [Column("oas_code", TypeName = "varchar(2)")]
    public string OasCode { get; set; } = string.Empty;

    [Column("obstacle_number", TypeName = "varchar(6)")]
    public string ObstacleNumber { get; set; } = string.Empty;

    [Column("verification_status", TypeName = "varchar(1)")]
    public string? VerificationStatus { get; set; }

    [Column("country_id", TypeName = "varchar(2)")]
    public string? CountryId { get; set; }

    [Column("state_id", TypeName = "varchar(2)")]
    public string? StateId { get; set; }

    [Column("city_name", TypeName = "varchar(16)")]
    public string? CityName { get; set; }

    [Column("lat_degrees")]
    public int? LatDegrees { get; set; }

    [Column("lat_minutes")]
    public int? LatMinutes { get; set; }

    [Column("lat_seconds", TypeName = "decimal(6,2)")]
    public decimal? LatSeconds { get; set; }

    [Column("lat_hemisphere", TypeName = "varchar(1)")]
    public string? LatHemisphere { get; set; }

    [Column("long_degrees")]
    public int? LongDegrees { get; set; }

    [Column("long_minutes")]
    public int? LongMinutes { get; set; }

    [Column("long_seconds", TypeName = "decimal(6,2)")]
    public decimal? LongSeconds { get; set; }

    [Column("long_hemisphere", TypeName = "varchar(1)")]
    public string? LongHemisphere { get; set; }

    [Column("lat_decimal", TypeName = "decimal(10,8)")]
    public decimal? LatDecimal { get; set; }

    [Column("long_decimal", TypeName = "decimal(11,8)")]
    public decimal? LongDecimal { get; set; }

    [Column("obstacle_type", TypeName = "varchar(18)")]
    public string? ObstacleType { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("height_agl")]
    public int? HeightAgl { get; set; }

    [Column("height_amsl")]
    public int? HeightAmsl { get; set; }

    [Column("lighting", TypeName = "varchar(1)")]
    public string? Lighting { get; set; }

    [Column("horizontal_accuracy", TypeName = "varchar(1)")]
    public string? HorizontalAccuracy { get; set; }

    [Column("vertical_accuracy", TypeName = "varchar(1)")]
    public string? VerticalAccuracy { get; set; }

    [Column("mark_indicator", TypeName = "varchar(1)")]
    public string? MarkIndicator { get; set; }

    [Column("faa_study_number", TypeName = "varchar(14)")]
    public string? FaaStudyNumber { get; set; }

    [Column("action", TypeName = "varchar(1)")]
    public string? Action { get; set; }

    [Column("julian_date", TypeName = "varchar(7)")]
    public string? JulianDate { get; set; }

    [Column("location", TypeName = "geography(Point, 4326)")]
    public Point? Location { get; set; }
}
