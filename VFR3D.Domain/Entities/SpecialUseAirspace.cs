using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VFR3D.Domain.Entities
{
    [Table("special_use_airspaces")]
    public class SpecialUseAirspace
    {
        [Key]
        [Column("global_id", TypeName = "varchar(50)")]
        public string GlobalId { get; set; } = string.Empty;

        [Column("name", TypeName = "varchar(200)")]
        public string? Name { get; set; }

        [Column("type_code", TypeName = "varchar(200)")]
        public string? TypeCode { get; set; }

        [Column("class", TypeName = "varchar(200)")]
        public string? Class { get; set; }

        [Column("upper_desc", TypeName = "varchar(200)")]
        public string? UpperDesc { get; set; }

        [Column("upper_val", TypeName = "varchar(200)")]
        public string? UpperVal { get; set; }

        [Column("upper_uom", TypeName = "varchar(200)")]
        public string? UpperUom { get; set; }

        [Column("upper_code", TypeName = "varchar(200)")]
        public string? UpperCode { get; set; }

        [Column("lower_desc", TypeName = "varchar(200)")]
        public string? LowerDesc { get; set; }

        [Column("lower_val", TypeName = "varchar(200)")]
        public string? LowerVal { get; set; }

        [Column("lower_uom", TypeName = "varchar(200)")]
        public string? LowerUom { get; set; }

        [Column("lower_code", TypeName = "varchar(200)")]
        public string? LowerCode { get; set; }

        [Column("level_code", TypeName = "varchar(200)")]
        public string? LevelCode { get; set; }

        [Column("city", TypeName = "varchar(254)")]
        public string? City { get; set; }

        [Column("state", TypeName = "varchar(254)")]
        public string? State { get; set; }

        [Column("country", TypeName = "varchar(254)")]
        public string? Country { get; set; }

        [Column("cont_agent", TypeName = "varchar(254)")]
        public string? ContAgent { get; set; }

        [Column("comm_name", TypeName = "varchar(200)")]
        public string? CommName { get; set; }

        [Column("sector", TypeName = "varchar(200)")]
        public string? Sector { get; set; }

        [Column("onshore", TypeName = "varchar(200)")]
        public string? Onshore { get; set; }

        [Column("exclusion", TypeName = "varchar(200)")]
        public string? Exclusion { get; set; }

        [Column("times_of_use", TypeName = "varchar(254)")]
        public string? TimesOfUse { get; set; }

        [Column("gmt_offset", TypeName = "varchar(200)")]
        public string? GmtOffset { get; set; }

        [Column("dst_code", TypeName = "varchar(200)")]
        public string? DstCode { get; set; }

        [Column("remarks", TypeName = "varchar(200)")]
        public string? Remarks { get; set; }

        [Column("ak_low")]
        public short? AkLow { get; set; }

        [Column("ak_high")]
        public short? AkHigh { get; set; }

        [Column("us_low")]
        public short? UsLow { get; set; }

        [Column("us_high")]
        public short? UsHigh { get; set; }

        [Column("us_area")]
        public short? UsArea { get; set; }

        [Column("pacific")]
        public short? Pacific { get; set; }

        [Column("shape_area")]
        public float? ShapeArea { get; set; }

        [Column("shape_length")]
        public float? ShapeLength { get; set; }

        [Column("geometry", TypeName = "geometry(Polygon, 4326)")]
        public Geometry? Geometry { get; set; }
    }
}
