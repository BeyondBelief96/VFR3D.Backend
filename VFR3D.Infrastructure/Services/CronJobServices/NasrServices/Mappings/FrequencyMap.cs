using CsvHelper.Configuration;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Utils;

namespace VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Mappings
{
    public sealed class FrequencyMap : ClassMap<CommunicationFrequency>
    {
        public FrequencyMap()
        {
            Map(m => m.FacilityCode).Name("FACILITY").Optional();
            Map(m => m.EffectiveDate).Name("EFF_DATE").TypeConverter<OptionalDateConverter>();
            Map(m => m.FacilityName).Name("FAC_NAME").Optional();
            Map(m => m.FacilityType).Name("FACILITY_TYPE");
            Map(m => m.ArtccOrFssId).Name("ARTCC_OR_FSS_ID").Optional();
            Map(m => m.Cpdlc).Name("CPDLC").Optional();
            Map(m => m.TowerHours).Name("TOWER_HRS").Optional();
            Map(m => m.ServicedFacility).Name("SERVICED_FACILITY");
            Map(m => m.ServicedFacilityName).Name("SERVICED_FAC_NAME").Optional();
            Map(m => m.ServicedSiteType).Name("SERVICED_SITE_TYPE").Optional();
            Map(m => m.Latitude).Name("LAT_DECIMAL").TypeConverter<OptionalDecimalConverter>();
            Map(m => m.Longitude).Name("LONG_DECIMAL").TypeConverter<OptionalDecimalConverter>();
            Map(m => m.ServicedCity).Name("SERVICED_CITY").Optional();
            Map(m => m.ServicedState).Name("SERVICED_STATE").Optional();
            Map(m => m.ServicedCountry).Name("SERVICED_COUNTRY").Optional();
            Map(m => m.TowerOrCommCall).Name("TOWER_OR_COMM_CALL").Optional();
            Map(m => m.PrimaryApproachRadioCall).Name("PRIMARY_APPROACH_RADIO_CALL").Optional();
            Map(m => m.Frequency).Name("FREQ").Optional();
            Map(m => m.Sectorization).Name("SECTORIZATION").Optional();
            Map(m => m.FrequencyUse).Name("FREQ_USE").Optional();
            Map(m => m.Remark).Name("REMARK").Optional();
        }
    }
}
