using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace VFR3D.Domain.Entities
{
    [Table("airspaces")]
    public class Airspace
    {
        [Key]
        [Column("global_id", TypeName = "varchar(50)")]
        public string GlobalId { get; set; } = string.Empty;

        [Column("ident", TypeName = "varchar(200)")]
        public string? Ident { get; set; }

        [Column("icao_id", TypeName = "varchar(200)")]
        public string? IcaoId { get; set; }

        [Column("name", TypeName = "varchar(200)")]
        public string? Name { get; set; }

        [Column("upper_desc", TypeName = "varchar(200)")]
        public string? UpperDesc { get; set; }

        [Column("upper_val")]
        public double? UpperVal { get; set; }

        [Column("upper_uom", TypeName = "varchar(200)")]
        public string? UpperUom { get; set; }

        [Column("upper_code", TypeName = "varchar(200)")]
        public string? UpperCode { get; set; }

        [Column("lower_desc", TypeName = "varchar(200)")]
        public string? LowerDesc { get; set; }

        [Column("lower_val")]
        public double? LowerVal { get; set; }

        [Column("lower_uom", TypeName = "varchar(200)")]
        public string? LowerUom { get; set; }

        [Column("lower_code", TypeName = "varchar(200)")]
        public string? LowerCode { get; set; }

        [Column("type_code", TypeName = "varchar(200)")]
        public string? TypeCode { get; set; }

        [Column("local_type", TypeName = "varchar(200)")]
        public string? LocalType { get; set; }

        [Column("class", TypeName = "varchar(200)")]
        public string? Class { get; set; }

        [Column("mil_code", TypeName = "varchar(200)")]
        public string? MilCode { get; set; }

        [Column("comm_name", TypeName = "varchar(200)")]
        public string? CommName { get; set; }

        [Column("level", TypeName = "varchar(200)")]
        public string? Level { get; set; }

        [Column("sector", TypeName = "varchar(200)")]
        public string? Sector { get; set; }

        [Column("onshore", TypeName = "varchar(200)")]
        public string? Onshore { get; set; }

        [Column("exclusion", TypeName = "varchar(200)")]
        public string? Exclusion { get; set; }

        [Column("wkhr_code", TypeName = "varchar(200)")]
        public string? WkhrCode { get; set; }

        [Column("wkhr_rmk", TypeName = "varchar(200)")]
        public string? WkhrRmk { get; set; }

        [Column("dst", TypeName = "varchar(200)")]
        public string? Dst { get; set; }

        [Column("gmt_offset", TypeName = "varchar(200)")]
        public string? GmtOffset { get; set; }

        [Column("cont_agent", TypeName = "varchar(254)")]
        public string? ContAgent { get; set; }

        [Column("city", TypeName = "varchar(254)")]
        public string? City { get; set; }

        [Column("state", TypeName = "varchar(254)")]
        public string? State { get; set; }

        [Column("country", TypeName = "varchar(254)")]
        public string? Country { get; set; }

        [Column("adhp_id", TypeName = "varchar(50)")]
        public string? AdhpId { get; set; }

        [Column("us_high")]
        public short? UsHigh { get; set; }

        [Column("ak_high")]
        public short? AkHigh { get; set; }

        [Column("ak_low")]
        public short? AkLow { get; set; }

        [Column("us_low")]
        public short? UsLow { get; set; }

        [Column("us_area")]
        public short? UsArea { get; set; }

        [Column("pacific")]
        public short? Pacific { get; set; }

        [Column("shape_area")]
        public double? ShapeArea { get; set; }

        [Column("shape_length")]
        public double? ShapeLength { get; set; }

        [Column("geometry", TypeName = "geometry(Polygon, 4326)")]
        public Geometry? Geometry { get; set; }
    }
}
