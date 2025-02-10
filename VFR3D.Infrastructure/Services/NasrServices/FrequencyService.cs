using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Enums;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.NasrServices.Mappings;

namespace VFR3D.Infrastructure.Services.NasrServices
{
    public class FrequencyService : FaaNasrBaseService<CommunicationFrequency>
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

        public FrequencyService(
            ILogger<FrequencyService> logger,
            IHttpClientFactory httpClientFactory,
            IFaaPublicationCycleService faaPublicationCycleService,
            CronServiceDbContext dbContext)
            : base(logger, httpClientFactory, faaPublicationCycleService, dbContext)
        {
        }
    }
}
