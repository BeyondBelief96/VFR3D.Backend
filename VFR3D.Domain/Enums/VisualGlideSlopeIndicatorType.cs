using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VisualGlideSlopeIndicatorType
{
    Unknown,
    None,
    // SAVASI - Simplified Abbreviated VASI
    Savasi2BoxLeft,
    Savasi2BoxRight,
    // VASI - Visual Approach Slope Indicator
    Vasi2BoxLeft,
    Vasi2BoxRight,
    Vasi4BoxLeft,
    Vasi4BoxRight,
    Vasi6BoxLeft,
    Vasi6BoxRight,
    Vasi12Box,
    Vasi16Box,
    // PAPI - Precision Approach Path Indicator
    Papi2LightLeft,
    Papi2LightRight,
    Papi4LightLeft,
    Papi4LightRight,
    // Tri-Color
    TriColorLeft,
    TriColorRight,
    // Pulsating/Steady Burning
    PulsatingLeft,
    PulsatingRight,
    // Panel Systems
    PanelLeft,
    PanelRight,
    // Other
    NonStandard,
    PrivateUse,
    NonSpecificVasi
}
