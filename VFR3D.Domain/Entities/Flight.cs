using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VFR3D.Domain.ValueObjects.Flights;

namespace VFR3D.Domain.Entities
{
    /// <summary>
    /// A flight represents a single flight plan for a given user, represented by a set of waypoints, a departure time, and a cruising altitude.
    /// </summary>
    [Table("flights")]
    public class Flight
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Required]
        [Column("auth0_user_id")]
        public string Auth0UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("departure_time")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Column("planned_cruising_altitude")]
        public int PlannedCruisingAltitude { get; set; }

        [Required]
        [Column("waypoints", TypeName = "jsonb")]
        public List<Waypoint> Waypoints { get; set; } = [];

        [Required]
        [Column("aircraft_performance_id")]
        public string AircraftPerformanceId { get; set; } = string.Empty;

        [Column("total_route_distance")]
        public double TotalRouteDistance { get; set; }

        [Column("total_route_time_hours")]
        public double TotalRouteTimeHours { get; set; }

        [Column("total_fuel_used")]
        public double TotalFuelUsed { get; set; }

        [Column("average_wind_component")]
        public double AverageWindComponent { get; set; }

        [Column("legs", TypeName = "jsonb")]
        public List<NavlogLeg> Legs { get; set; } = [];

        [Column("state_codes_along_route", TypeName = "jsonb")]
        public List<string> StateCodesAlongRoute { get; set; } = [];

        [Column("airspace_global_ids", TypeName = "jsonb")]
        public List<string>? AirspaceGlobalIds { get; set; } = [];

        [Column("special_use_airspace_global_ids", TypeName = "jsonb")]
        public List<string>? SpecialUseAirspaceGlobalIds { get; set; } = [];

        [Column("related_flight_id")]
        public string? RelatedFlightId { get; set; }

        [Column("obstacle_oas_numbers", TypeName = "jsonb")]
        public List<string>? ObstacleOasNumbers { get; set; } = [];

        [Column("aircraft_id")]
        public string? AircraftId { get; set; }

        // Navigation Properties
        public virtual Aircraft? Aircraft { get; set; }

        public virtual AircraftPerformanceProfile? AircraftPerformanceProfile { get; set; }

        public virtual WeightBalanceCalculation? WeightBalanceCalculation { get; set; }
    }
}