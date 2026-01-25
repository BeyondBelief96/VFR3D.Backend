using VFR3D.Domain.Entities;
using VFR3D.Domain.Enums;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class GAirmetMapper
{
    public static GAirmetDto ToDto(GAirmet gairmet)
    {
        return new GAirmetDto
        {
            Id = gairmet.Id,
            ReceiptTime = gairmet.ReceiptTime,
            IssueTime = gairmet.IssueTime,
            ExpireTime = gairmet.ExpireTime,
            ValidTime = gairmet.ValidTime,
            Product = ParseProduct(gairmet.Product),
            Tag = gairmet.Tag,
            ForecastHour = gairmet.ForecastHour,
            Hazard = ParseHazardType(gairmet.HazardType),
            HazardSeverity = gairmet.HazardSeverity,
            GeometryType = gairmet.GeometryType,
            DueTo = gairmet.DueTo,
            Altitudes = gairmet.Altitudes,
            Area = gairmet.Area
        };
    }

    private static GAirmetProduct ParseProduct(string product)
    {
        return product.ToUpperInvariant() switch
        {
            "SIERRA" => GAirmetProduct.SIERRA,
            "TANGO" => GAirmetProduct.TANGO,
            "ZULU" => GAirmetProduct.ZULU,
            _ => GAirmetProduct.SIERRA // Default fallback
        };
    }

    public static GAirmetHazardType? ParseHazardType(string? hazardType)
    {
        if (string.IsNullOrEmpty(hazardType)) return null;

        return hazardType.ToUpperInvariant() switch
        {
            // SIERRA hazards
            "MT_OBSC" => GAirmetHazardType.MT_OBSC,
            "IFR" => GAirmetHazardType.IFR,

            // TANGO hazards
            "TURB-LO" => GAirmetHazardType.TURB_LO,
            "TURB-HI" => GAirmetHazardType.TURB_HI,
            "LLWS" => GAirmetHazardType.LLWS,
            "SFC_WIND" => GAirmetHazardType.SFC_WIND,
            "SFC-WIND" => GAirmetHazardType.SFC_WIND, // Handle both formats

            // ZULU hazards
            "ICE" => GAirmetHazardType.ICE,
            "FZLVL" => GAirmetHazardType.FZLVL,
            "M_FZLVL" => GAirmetHazardType.M_FZLVL,

            _ => null
        };
    }

    public static string? ConvertHazardTypeToString(GAirmetHazardType hazardType)
    {
        return hazardType switch
        {
            // SIERRA hazards
            GAirmetHazardType.MT_OBSC => "MT_OBSC",
            GAirmetHazardType.IFR => "IFR",

            // TANGO hazards
            GAirmetHazardType.TURB_LO => "TURB-LO",
            GAirmetHazardType.TURB_HI => "TURB-HI",
            GAirmetHazardType.LLWS => "LLWS",
            GAirmetHazardType.SFC_WIND => "SFC_WIND",

            // ZULU hazards
            GAirmetHazardType.ICE => "ICE",
            GAirmetHazardType.FZLVL => "FZLVL",
            GAirmetHazardType.M_FZLVL => "M_FZLVL",

            _ => null
        };
    }
}
