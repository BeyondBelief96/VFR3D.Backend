using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers;

public static class CommunicationFrequencyMapper
{
    public static CommunicationFrequencyDto ToDto(CommunicationFrequency frequency)
    {
        return new CommunicationFrequencyDto
        {
            Id = frequency.Id,
            FacilityCode = frequency.FacilityCode,
            EffectiveDate = frequency.EffectiveDate,
            FacilityName = frequency.FacilityName,
            FacilityType = frequency.FacilityType,
            ArtccOrFssId = frequency.ArtccOrFssId,
            Cpdlc = frequency.Cpdlc,
            TowerHours = frequency.TowerHours,
            ServicedFacility = frequency.ServicedFacility,
            ServicedFacilityName = frequency.ServicedFacilityName,
            ServicedSiteType = frequency.ServicedSiteType,
            Latitude = frequency.Latitude,
            Longitude = frequency.Longitude,
            ServicedCity = frequency.ServicedCity,
            ServicedState = frequency.ServicedState,
            ServicedCountry = frequency.ServicedCountry,
            TowerOrCommCall = frequency.TowerOrCommCall,
            PrimaryApproachRadioCall = frequency.PrimaryApproachRadioCall,
            Frequency = frequency.Frequency,
            Sectorization = frequency.Sectorization,
            FrequencyUse = frequency.FrequencyUse,
            Remark = frequency.Remark
        };
    }
}