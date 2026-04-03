using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VFR3D.Domain.Entities;

[Table("runway_ends")]
public class RunwayEnd : INasrEntity<RunwayEnd>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("site_no", TypeName = "varchar(9)")]
    [Required]
    public string SiteNo { get; set; } = string.Empty;

    [Column("runway_id_ref", TypeName = "varchar(7)")]
    [Required]
    public string RunwayIdRef { get; set; } = string.Empty;

    [Column("runway_fk")]
    public Guid? RunwayFk { get; set; }

    [Column("runway_end_id", TypeName = "varchar(3)")]
    [Required]
    public string RunwayEndId { get; set; } = string.Empty;

    [Column("true_alignment")]
    public int? TrueAlignment { get; set; }

    [Column("approach_type", TypeName = "varchar(10)")]
    public string? ApproachType { get; set; }

    [Column("right_hand_traffic_pattern")]
    public bool RightHandTrafficPattern { get; set; }

    [Column("runway_markings_type", TypeName = "varchar(5)")]
    public string? RunwayMarkingsType { get; set; }

    [Column("runway_markings_condition", TypeName = "varchar(1)")]
    public string? RunwayMarkingsCondition { get; set; }

    [Column("lat_decimal", TypeName = "decimal(10,8)")]
    public decimal? LatDecimal { get; set; }

    [Column("long_decimal", TypeName = "decimal(11,8)")]
    public decimal? LongDecimal { get; set; }

    [Column("elevation", TypeName = "decimal(7,1)")]
    public decimal? Elevation { get; set; }

    [Column("threshold_crossing_height", TypeName = "decimal(5,1)")]
    public decimal? ThresholdCrossingHeight { get; set; }

    [Column("visual_glide_path_angle", TypeName = "decimal(4,2)")]
    public decimal? VisualGlidePathAngle { get; set; }

    [Column("displaced_threshold_lat_decimal", TypeName = "decimal(10,8)")]
    public decimal? DisplacedThresholdLatDecimal { get; set; }

    [Column("displaced_threshold_long_decimal", TypeName = "decimal(11,8)")]
    public decimal? DisplacedThresholdLongDecimal { get; set; }

    [Column("displaced_threshold_elev", TypeName = "decimal(7,1)")]
    public decimal? DisplacedThresholdElev { get; set; }

    [Column("displaced_threshold_length")]
    public int? DisplacedThresholdLength { get; set; }

    [Column("touchdown_zone_elev", TypeName = "decimal(7,1)")]
    public decimal? TouchdownZoneElev { get; set; }

    [Column("visual_glide_slope_indicator", TypeName = "varchar(5)")]
    public string? VisualGlideSlopeIndicator { get; set; }

    [Column("runway_visual_range_equipment", TypeName = "varchar(3)")]
    public string? RunwayVisualRangeEquipment { get; set; }

    [Column("runway_visibility_value_equipment")]
    public bool RunwayVisibilityValueEquipment { get; set; }

    [Column("approach_light_system", TypeName = "varchar(8)")]
    public string? ApproachLightSystem { get; set; }

    [Column("runway_end_lights")]
    public bool RunwayEndLights { get; set; }

    [Column("centerline_lights")]
    public bool CenterlineLights { get; set; }

    [Column("touchdown_zone_lights")]
    public bool TouchdownZoneLights { get; set; }

    [Column("controlling_object_description", TypeName = "varchar(11)")]
    public string? ControllingObjectDescription { get; set; }

    [Column("controlling_object_marked_lighted", TypeName = "varchar(4)")]
    public string? ControllingObjectMarkedLighted { get; set; }

    [Column("controlling_object_clearance_slope")]
    public int? ControllingObjectClearanceSlope { get; set; }

    [Column("controlling_object_height_above_runway")]
    public int? ControllingObjectHeightAboveRunway { get; set; }

    [Column("controlling_object_distance_from_runway")]
    public int? ControllingObjectDistanceFromRunway { get; set; }

    [Column("controlling_object_centerline_offset", TypeName = "varchar(7)")]
    public string? ControllingObjectCenterlineOffset { get; set; }

    // Navigation property
    [ForeignKey(nameof(RunwayFk))]
    public virtual Runway? Runway { get; set; }

    // INasrEntity<RunwayEnd> implementation
    public string CreateUniqueKey()
    {
        return string.Join("|", SiteNo, RunwayIdRef, RunwayEndId);
    }

    public void UpdateFrom(RunwayEnd source, HashSet<string>? limitToProperties = null)
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

    public RunwayEnd CreateSelectiveEntity(HashSet<string> properties)
    {
        var selective = new RunwayEnd();

        if (properties.Contains(nameof(SiteNo)))
            selective.SiteNo = SiteNo;
        if (properties.Contains(nameof(RunwayIdRef)))
            selective.RunwayIdRef = RunwayIdRef;
        if (properties.Contains(nameof(RunwayEndId)))
            selective.RunwayEndId = RunwayEndId;
        if (properties.Contains(nameof(TrueAlignment)))
            selective.TrueAlignment = TrueAlignment;
        if (properties.Contains(nameof(ApproachType)))
            selective.ApproachType = ApproachType;
        if (properties.Contains(nameof(RightHandTrafficPattern)))
            selective.RightHandTrafficPattern = RightHandTrafficPattern;
        if (properties.Contains(nameof(RunwayMarkingsType)))
            selective.RunwayMarkingsType = RunwayMarkingsType;
        if (properties.Contains(nameof(RunwayMarkingsCondition)))
            selective.RunwayMarkingsCondition = RunwayMarkingsCondition;
        if (properties.Contains(nameof(LatDecimal)))
            selective.LatDecimal = LatDecimal;
        if (properties.Contains(nameof(LongDecimal)))
            selective.LongDecimal = LongDecimal;
        if (properties.Contains(nameof(Elevation)))
            selective.Elevation = Elevation;
        if (properties.Contains(nameof(ThresholdCrossingHeight)))
            selective.ThresholdCrossingHeight = ThresholdCrossingHeight;
        if (properties.Contains(nameof(VisualGlidePathAngle)))
            selective.VisualGlidePathAngle = VisualGlidePathAngle;
        if (properties.Contains(nameof(DisplacedThresholdLatDecimal)))
            selective.DisplacedThresholdLatDecimal = DisplacedThresholdLatDecimal;
        if (properties.Contains(nameof(DisplacedThresholdLongDecimal)))
            selective.DisplacedThresholdLongDecimal = DisplacedThresholdLongDecimal;
        if (properties.Contains(nameof(DisplacedThresholdElev)))
            selective.DisplacedThresholdElev = DisplacedThresholdElev;
        if (properties.Contains(nameof(DisplacedThresholdLength)))
            selective.DisplacedThresholdLength = DisplacedThresholdLength;
        if (properties.Contains(nameof(TouchdownZoneElev)))
            selective.TouchdownZoneElev = TouchdownZoneElev;
        if (properties.Contains(nameof(VisualGlideSlopeIndicator)))
            selective.VisualGlideSlopeIndicator = VisualGlideSlopeIndicator;
        if (properties.Contains(nameof(RunwayVisualRangeEquipment)))
            selective.RunwayVisualRangeEquipment = RunwayVisualRangeEquipment;
        if (properties.Contains(nameof(RunwayVisibilityValueEquipment)))
            selective.RunwayVisibilityValueEquipment = RunwayVisibilityValueEquipment;
        if (properties.Contains(nameof(ApproachLightSystem)))
            selective.ApproachLightSystem = ApproachLightSystem;
        if (properties.Contains(nameof(RunwayEndLights)))
            selective.RunwayEndLights = RunwayEndLights;
        if (properties.Contains(nameof(CenterlineLights)))
            selective.CenterlineLights = CenterlineLights;
        if (properties.Contains(nameof(TouchdownZoneLights)))
            selective.TouchdownZoneLights = TouchdownZoneLights;
        if (properties.Contains(nameof(ControllingObjectDescription)))
            selective.ControllingObjectDescription = ControllingObjectDescription;
        if (properties.Contains(nameof(ControllingObjectMarkedLighted)))
            selective.ControllingObjectMarkedLighted = ControllingObjectMarkedLighted;
        if (properties.Contains(nameof(ControllingObjectClearanceSlope)))
            selective.ControllingObjectClearanceSlope = ControllingObjectClearanceSlope;
        if (properties.Contains(nameof(ControllingObjectHeightAboveRunway)))
            selective.ControllingObjectHeightAboveRunway = ControllingObjectHeightAboveRunway;
        if (properties.Contains(nameof(ControllingObjectDistanceFromRunway)))
            selective.ControllingObjectDistanceFromRunway = ControllingObjectDistanceFromRunway;
        if (properties.Contains(nameof(ControllingObjectCenterlineOffset)))
            selective.ControllingObjectCenterlineOffset = ControllingObjectCenterlineOffset;

        return selective;
    }

    private void UpdateAllProperties(RunwayEnd source)
    {
        TrueAlignment = source.TrueAlignment;
        ApproachType = source.ApproachType;
        RightHandTrafficPattern = source.RightHandTrafficPattern;
        RunwayMarkingsType = source.RunwayMarkingsType;
        RunwayMarkingsCondition = source.RunwayMarkingsCondition;
        LatDecimal = source.LatDecimal;
        LongDecimal = source.LongDecimal;
        Elevation = source.Elevation;
        ThresholdCrossingHeight = source.ThresholdCrossingHeight;
        VisualGlidePathAngle = source.VisualGlidePathAngle;
        DisplacedThresholdLatDecimal = source.DisplacedThresholdLatDecimal;
        DisplacedThresholdLongDecimal = source.DisplacedThresholdLongDecimal;
        DisplacedThresholdElev = source.DisplacedThresholdElev;
        DisplacedThresholdLength = source.DisplacedThresholdLength;
        TouchdownZoneElev = source.TouchdownZoneElev;
        VisualGlideSlopeIndicator = source.VisualGlideSlopeIndicator;
        RunwayVisualRangeEquipment = source.RunwayVisualRangeEquipment;
        RunwayVisibilityValueEquipment = source.RunwayVisibilityValueEquipment;
        ApproachLightSystem = source.ApproachLightSystem;
        RunwayEndLights = source.RunwayEndLights;
        CenterlineLights = source.CenterlineLights;
        TouchdownZoneLights = source.TouchdownZoneLights;
        ControllingObjectDescription = source.ControllingObjectDescription;
        ControllingObjectMarkedLighted = source.ControllingObjectMarkedLighted;
        ControllingObjectClearanceSlope = source.ControllingObjectClearanceSlope;
        ControllingObjectHeightAboveRunway = source.ControllingObjectHeightAboveRunway;
        ControllingObjectDistanceFromRunway = source.ControllingObjectDistanceFromRunway;
        ControllingObjectCenterlineOffset = source.ControllingObjectCenterlineOffset;
    }

    private void UpdateSelectiveProperties(RunwayEnd source, HashSet<string> limitToProperties)
    {
        if (limitToProperties.Contains(nameof(TrueAlignment)) && source.TrueAlignment != null)
            TrueAlignment = source.TrueAlignment;
        if (limitToProperties.Contains(nameof(ApproachType)) && source.ApproachType != null)
            ApproachType = source.ApproachType;
        if (limitToProperties.Contains(nameof(RightHandTrafficPattern)))
            RightHandTrafficPattern = source.RightHandTrafficPattern;
        if (limitToProperties.Contains(nameof(RunwayMarkingsType)) && source.RunwayMarkingsType != null)
            RunwayMarkingsType = source.RunwayMarkingsType;
        if (limitToProperties.Contains(nameof(RunwayMarkingsCondition)) && source.RunwayMarkingsCondition != null)
            RunwayMarkingsCondition = source.RunwayMarkingsCondition;
        if (limitToProperties.Contains(nameof(LatDecimal)) && source.LatDecimal != null)
            LatDecimal = source.LatDecimal;
        if (limitToProperties.Contains(nameof(LongDecimal)) && source.LongDecimal != null)
            LongDecimal = source.LongDecimal;
        if (limitToProperties.Contains(nameof(Elevation)) && source.Elevation != null)
            Elevation = source.Elevation;
        if (limitToProperties.Contains(nameof(ThresholdCrossingHeight)) && source.ThresholdCrossingHeight != null)
            ThresholdCrossingHeight = source.ThresholdCrossingHeight;
        if (limitToProperties.Contains(nameof(VisualGlidePathAngle)) && source.VisualGlidePathAngle != null)
            VisualGlidePathAngle = source.VisualGlidePathAngle;
        if (limitToProperties.Contains(nameof(DisplacedThresholdLatDecimal)) && source.DisplacedThresholdLatDecimal != null)
            DisplacedThresholdLatDecimal = source.DisplacedThresholdLatDecimal;
        if (limitToProperties.Contains(nameof(DisplacedThresholdLongDecimal)) && source.DisplacedThresholdLongDecimal != null)
            DisplacedThresholdLongDecimal = source.DisplacedThresholdLongDecimal;
        if (limitToProperties.Contains(nameof(DisplacedThresholdElev)) && source.DisplacedThresholdElev != null)
            DisplacedThresholdElev = source.DisplacedThresholdElev;
        if (limitToProperties.Contains(nameof(DisplacedThresholdLength)) && source.DisplacedThresholdLength != null)
            DisplacedThresholdLength = source.DisplacedThresholdLength;
        if (limitToProperties.Contains(nameof(TouchdownZoneElev)) && source.TouchdownZoneElev != null)
            TouchdownZoneElev = source.TouchdownZoneElev;
        if (limitToProperties.Contains(nameof(VisualGlideSlopeIndicator)) && source.VisualGlideSlopeIndicator != null)
            VisualGlideSlopeIndicator = source.VisualGlideSlopeIndicator;
        if (limitToProperties.Contains(nameof(RunwayVisualRangeEquipment)) && source.RunwayVisualRangeEquipment != null)
            RunwayVisualRangeEquipment = source.RunwayVisualRangeEquipment;
        if (limitToProperties.Contains(nameof(RunwayVisibilityValueEquipment)))
            RunwayVisibilityValueEquipment = source.RunwayVisibilityValueEquipment;
        if (limitToProperties.Contains(nameof(ApproachLightSystem)) && source.ApproachLightSystem != null)
            ApproachLightSystem = source.ApproachLightSystem;
        if (limitToProperties.Contains(nameof(RunwayEndLights)))
            RunwayEndLights = source.RunwayEndLights;
        if (limitToProperties.Contains(nameof(CenterlineLights)))
            CenterlineLights = source.CenterlineLights;
        if (limitToProperties.Contains(nameof(TouchdownZoneLights)))
            TouchdownZoneLights = source.TouchdownZoneLights;
        if (limitToProperties.Contains(nameof(ControllingObjectDescription)) && source.ControllingObjectDescription != null)
            ControllingObjectDescription = source.ControllingObjectDescription;
        if (limitToProperties.Contains(nameof(ControllingObjectMarkedLighted)) && source.ControllingObjectMarkedLighted != null)
            ControllingObjectMarkedLighted = source.ControllingObjectMarkedLighted;
        if (limitToProperties.Contains(nameof(ControllingObjectClearanceSlope)) && source.ControllingObjectClearanceSlope != null)
            ControllingObjectClearanceSlope = source.ControllingObjectClearanceSlope;
        if (limitToProperties.Contains(nameof(ControllingObjectHeightAboveRunway)) && source.ControllingObjectHeightAboveRunway != null)
            ControllingObjectHeightAboveRunway = source.ControllingObjectHeightAboveRunway;
        if (limitToProperties.Contains(nameof(ControllingObjectDistanceFromRunway)) && source.ControllingObjectDistanceFromRunway != null)
            ControllingObjectDistanceFromRunway = source.ControllingObjectDistanceFromRunway;
        if (limitToProperties.Contains(nameof(ControllingObjectCenterlineOffset)) && source.ControllingObjectCenterlineOffset != null)
            ControllingObjectCenterlineOffset = source.ControllingObjectCenterlineOffset;
    }
}
