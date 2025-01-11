using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VFR3D.Domain.Entities
{
    [Table("chart_supplement")]
    public class ChartSupplement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("airport_name")]
        public string? AirportName { get; set; }

        [Column("airport_city")]
        public string? AirportCity { get; set; }

        [Column("airport_code")]
        public string? AirportCode { get; set; }

        [Column("navigational_aid_name")]
        public string? NavigationalAidName { get; set; }

        [Column("file_name")]
        public string? FileName { get; set; }
    }
}
