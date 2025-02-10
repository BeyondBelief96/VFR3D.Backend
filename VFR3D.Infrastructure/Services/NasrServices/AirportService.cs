using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Enums;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.NasrServices.Mappings;

namespace VFR3D.Infrastructure.Services.NasrServices
{
    public class AirportService : FaaNasrBaseService<Airport>
    {
        protected override NasrDataType DataType => NasrDataType.APT;
        protected override string[] UniqueIdentifiers => new[] { "SiteNo" };
        protected override IEnumerable<(string FileName, Type ClassMap, bool IsBaseData)> CsvMappings =>
        new[]
        {
            ("APT_BASE.csv", typeof(AirportBaseMap), true),
            ("APT_ATT.csv", typeof(AirportAttendanceMap), false),
            ("APT_CON.csv", typeof(AirportContactMap), false)
        };

        public AirportService(
            ILogger<AirportService> logger,
            IHttpClientFactory httpClientFactory,
            IFaaPublicationCycleService faaPublicationCycleService,
            CronServiceDbContext dbContext)
            : base(logger, httpClientFactory, faaPublicationCycleService, dbContext)
        {
        }
    }
}