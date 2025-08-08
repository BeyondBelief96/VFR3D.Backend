using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Enums;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Mappings;

namespace VFR3D.Infrastructure.Services.CronJobServices.NasrServices
{
    public class CommunicationFrequencyCronService : FaaNasrBaseService<CommunicationFrequency>
    {
        protected override NasrDataType DataType => NasrDataType.FRQ;
        protected override string[] UniqueIdentifiers => new[] {
            "FacilityCode",
            "ServicedFacility",
            "ServicedSiteType",
            "ServicedState",
            "Frequency",
            "FrequencyUse",
            "Sectorization"
        };
        protected override IEnumerable<(string FileName, Type ClassMap, bool IsBaseData)> CsvMappings =>
        new[]
        {
            ("FRQ.csv", typeof(FrequencyMap), true),
        };

        protected override bool UsesLegacySiteNoDeduplication => false;

        protected override PublicationType PublicationType => PublicationType.NasrSubscription_Frequencies;

        public CommunicationFrequencyCronService(
            ILogger<CommunicationFrequencyCronService> logger,
            IHttpClientFactory httpClientFactory,
            IFaaPublicationCycleService faaPublicationCycleService,
            VFR3DDbContext dbContext)
            : base(logger, httpClientFactory, faaPublicationCycleService, dbContext)
        {
        }
    }
}
