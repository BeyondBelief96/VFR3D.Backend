using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VFR3D.Domain.Entities;

[Table("runways")]
public class Runway : INasrEntity<Runway>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("site_no", TypeName = "varchar(9)")]
    [Required]
    public string SiteNo { get; set; } = string.Empty;

    [Column("runway_id", TypeName = "varchar(7)")]
    [Required]
    public string RunwayId { get; set; } = string.Empty;

    [Column("length")]
    public int? Length { get; set; }

    [Column("width")]
    public int? Width { get; set; }

    [Column("surface_type_code", TypeName = "varchar(12)")]
    public string? SurfaceTypeCode { get; set; }

    [Column("surface_treatment_code", TypeName = "varchar(5)")]
    public string? SurfaceTreatmentCode { get; set; }

    [Column("pavement_classification", TypeName = "varchar(11)")]
    public string? PavementClassification { get; set; }

    [Column("edge_light_intensity", TypeName = "varchar(5)")]
    public string? EdgeLightIntensity { get; set; }

    [Column("weight_bearing_single_wheel")]
    public int? WeightBearingSingleWheel { get; set; }

    [Column("weight_bearing_dual_wheel")]
    public int? WeightBearingDualWheel { get; set; }

    [Column("weight_bearing_dual_tandem")]
    public int? WeightBearingDualTandem { get; set; }

    [Column("weight_bearing_double_dual_tandem")]
    public int? WeightBearingDoubleDualTandem { get; set; }

    // Navigation property
    public virtual ICollection<RunwayEnd> RunwayEnds { get; set; } = new List<RunwayEnd>();

    // INasrEntity<Runway> implementation
    public string CreateUniqueKey()
    {
        return string.Join("|", SiteNo, RunwayId);
    }

    public void UpdateFrom(Runway source, HashSet<string>? limitToProperties = null)
    {
        if (limitToProperties == null || !limitToProperties.Any())
        {
            UpdateAllProperties(source);
        }
        else
        {
            UpdateSelectiveProperties(source, limitToProperties);
        }
    }

    public Runway CreateSelectiveEntity(HashSet<string> properties)
    {
        var selective = new Runway();

        if (properties.Contains(nameof(SiteNo)))
            selective.SiteNo = SiteNo;
        if (properties.Contains(nameof(RunwayId)))
            selective.RunwayId = RunwayId;
        if (properties.Contains(nameof(Length)))
            selective.Length = Length;
        if (properties.Contains(nameof(Width)))
            selective.Width = Width;
        if (properties.Contains(nameof(SurfaceTypeCode)))
            selective.SurfaceTypeCode = SurfaceTypeCode;
        if (properties.Contains(nameof(SurfaceTreatmentCode)))
            selective.SurfaceTreatmentCode = SurfaceTreatmentCode;
        if (properties.Contains(nameof(PavementClassification)))
            selective.PavementClassification = PavementClassification;
        if (properties.Contains(nameof(EdgeLightIntensity)))
            selective.EdgeLightIntensity = EdgeLightIntensity;
        if (properties.Contains(nameof(WeightBearingSingleWheel)))
            selective.WeightBearingSingleWheel = WeightBearingSingleWheel;
        if (properties.Contains(nameof(WeightBearingDualWheel)))
            selective.WeightBearingDualWheel = WeightBearingDualWheel;
        if (properties.Contains(nameof(WeightBearingDualTandem)))
            selective.WeightBearingDualTandem = WeightBearingDualTandem;
        if (properties.Contains(nameof(WeightBearingDoubleDualTandem)))
            selective.WeightBearingDoubleDualTandem = WeightBearingDoubleDualTandem;

        return selective;
    }

    private void UpdateAllProperties(Runway source)
    {
        Length = source.Length;
        Width = source.Width;
        SurfaceTypeCode = source.SurfaceTypeCode;
        SurfaceTreatmentCode = source.SurfaceTreatmentCode;
        PavementClassification = source.PavementClassification;
        EdgeLightIntensity = source.EdgeLightIntensity;
        WeightBearingSingleWheel = source.WeightBearingSingleWheel;
        WeightBearingDualWheel = source.WeightBearingDualWheel;
        WeightBearingDualTandem = source.WeightBearingDualTandem;
        WeightBearingDoubleDualTandem = source.WeightBearingDoubleDualTandem;
    }

    private void UpdateSelectiveProperties(Runway source, HashSet<string> limitToProperties)
    {
        if (limitToProperties.Contains(nameof(Length)) && source.Length != null)
            Length = source.Length;
        if (limitToProperties.Contains(nameof(Width)) && source.Width != null)
            Width = source.Width;
        if (limitToProperties.Contains(nameof(SurfaceTypeCode)) && source.SurfaceTypeCode != null)
            SurfaceTypeCode = source.SurfaceTypeCode;
        if (limitToProperties.Contains(nameof(SurfaceTreatmentCode)) && source.SurfaceTreatmentCode != null)
            SurfaceTreatmentCode = source.SurfaceTreatmentCode;
        if (limitToProperties.Contains(nameof(PavementClassification)) && source.PavementClassification != null)
            PavementClassification = source.PavementClassification;
        if (limitToProperties.Contains(nameof(EdgeLightIntensity)) && source.EdgeLightIntensity != null)
            EdgeLightIntensity = source.EdgeLightIntensity;
        if (limitToProperties.Contains(nameof(WeightBearingSingleWheel)) && source.WeightBearingSingleWheel != null)
            WeightBearingSingleWheel = source.WeightBearingSingleWheel;
        if (limitToProperties.Contains(nameof(WeightBearingDualWheel)) && source.WeightBearingDualWheel != null)
            WeightBearingDualWheel = source.WeightBearingDualWheel;
        if (limitToProperties.Contains(nameof(WeightBearingDualTandem)) && source.WeightBearingDualTandem != null)
            WeightBearingDualTandem = source.WeightBearingDualTandem;
        if (limitToProperties.Contains(nameof(WeightBearingDoubleDualTandem)) && source.WeightBearingDoubleDualTandem != null)
            WeightBearingDoubleDualTandem = source.WeightBearingDoubleDualTandem;
    }
}
