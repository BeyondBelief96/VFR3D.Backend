using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VFR3D.Domain.Entities
{
    public class AirportDiagram
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string AirportName { get; set; } = string.Empty;

        [MaxLength(4)]
        public string? IcaoIdent { get; set; }

        [MaxLength(4)]
        public string? AirportIdent { get; set; }

        [MaxLength(100)]
        public string? ChartName { get; set; }

        [Required]
        [MaxLength(100)]
        public string FileName { get; set; } = string.Empty;
    }
}
