using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Enums;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Mappings;

namespace VFR3D.Infrastructure.Services.CronJobServices.NasrServices;

public class RunwayCronService : FaaNasrBaseService<Runway>
{
    protected override NasrDataType DataType => NasrDataType.APT;
    protected override string[] UniqueIdentifiers => new[] { "SiteNo", "RunwayId" };
    protected override IEnumerable<(string FileName, Type ClassMap, bool IsBaseData)> CsvMappings =>
        new[]
        {
            ("APT_RWY.csv", typeof(RunwayMap), true),
        };

    protected override bool UsesLegacySiteNoDeduplication => false;

    protected override PublicationType PublicationType => PublicationType.NasrSubscription_Airport;

    public RunwayCronService(
        ILogger<RunwayCronService> logger,
        IHttpClientFactory httpClientFactory,
        IFaaPublicationCycleService faaPublicationCycleService,
        VFR3DDbContext dbContext)
        : base(logger, httpClientFactory, faaPublicationCycleService, dbContext)
    {
    }
}
