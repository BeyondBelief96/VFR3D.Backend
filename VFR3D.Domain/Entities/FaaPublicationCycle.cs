using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using VFR3D.Domain.ValueObjects.FaaPublications;

namespace VFR3D.Domain.Entities
{
    [Table("faa_publication_cycle")]
    public class FaaPublicationCycle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("publication_type")]
        public PublicationType PublicationType { get; set; }

        [Column("cycle_length_days")]
        public int CycleLengthDays { get; set; } 

        [Column("known_valid_date")]
        public DateTime KnownValidDate { get; set; }

        [Column("last_successful_update")]
        public DateTime? LastSuccessfulUpdate { get; set; }
    }
}
