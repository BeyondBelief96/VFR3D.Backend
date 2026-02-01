using System.Text.Json.Serialization;

namespace VFR3D.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DocumentCategory
{
    POH,           // Pilot's Operating Handbook
    Manual,        // Equipment manuals, avionics guides
    Checklist,     // Custom checklists
    Maintenance,   // Maintenance records, logs
    Insurance,     // Insurance documents
    Registration,  // Aircraft registration
    Other          // Catch-all
}
