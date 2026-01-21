using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices.Models;

namespace VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices
{
    public class AirspaceCronService : ArcGisBaseService<Airspace>, IAirspaceCronService<Airspace>
    {
        protected override string BaseUrl => "https://services6.arcgis.com/ssFJjBXIUyZDrSYZ/arcgis/rest/services/Class_Airspace/FeatureServer/0/query";

        public AirspaceCronService(
            ILogger<AirspaceCronService> logger,
            IHttpClientFactory httpClientFactory,
            VFR3DDbContext dbContext)
            : base(logger, httpClientFactory, dbContext)
        {
        }

        public async Task UpdateAirspacesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var airspaceClass in new[] { "B", "C", "D" })
            {
                await UpdateAirspacesByClass(airspaceClass, cancellationToken);
            }
        }

        private async Task UpdateAirspacesByClass(string airspaceClass, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Class {Class} airspaces", airspaceClass);

            var parameters = new Dictionary<string, string>
            {
                ["where"] = $"CLASS = '{airspaceClass}'",
                ["outFields"] = "*"
            };

            var response = await QueryFeatures<AirspaceModel>(parameters, cancellationToken);

            // Extract valid GlobalIds from response
            var globalIds = response.Features
                .Select(f => f.Attributes.GlobalId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            // Batch load existing airspaces to avoid N+1 queries
            var existingAirspaces = await _dbContext.Airspaces
                .Where(a => globalIds.Contains(a.GlobalId))
                .ToDictionaryAsync(a => a.GlobalId, cancellationToken);

            var newAirspaces = new List<Airspace>();
            var skippedCount = 0;

            foreach (var feature in response.Features)
            {
                var globalId = feature.Attributes.GlobalId ?? string.Empty;
                if (string.IsNullOrEmpty(globalId))
                {
                    skippedCount++;
                    continue;
                }

                if (existingAirspaces.TryGetValue(globalId, out var existingAirspace))
                {
                    MapFieldsToEntity(existingAirspace, feature.Attributes);
                    existingAirspace.Geometry = CreatePolygonFromRings(feature.Geometry?.Rings ?? Array.Empty<List<double[]>>());
                }
                else
                {
                    var newAirspace = new Airspace { GlobalId = globalId };
                    MapFieldsToEntity(newAirspace, feature.Attributes);
                    newAirspace.Geometry = CreatePolygonFromRings(feature.Geometry?.Rings ?? Array.Empty<List<double[]>>());
                    newAirspaces.Add(newAirspace);
                }
            }

            if (newAirspaces.Count > 0)
            {
                await _dbContext.Airspaces.AddRangeAsync(newAirspaces, cancellationToken);
                _logger.LogDebug("Adding {Count} new Class {Class} airspaces", newAirspaces.Count, airspaceClass);
            }

            if (skippedCount > 0)
            {
                _logger.LogWarning("Skipped {Count} airspaces with null or empty GlobalId", skippedCount);
            }

            var changesCount = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Processed {Count} Class {Class} airspaces: {NewCount} new, {UpdatedCount} updated, {Changes} database changes",
                response.Features.Count, airspaceClass, newAirspaces.Count, existingAirspaces.Count, changesCount);
        }

        protected override async Task<Airspace?> FindExistingEntity(object id, CancellationToken cancellationToken)
        {
            return await _dbContext.Airspaces.FirstOrDefaultAsync(a => a.GlobalId == (string)id, cancellationToken);
        }

        protected override Airspace CreateNewEntity(object id)
        {
            return new Airspace { GlobalId = (string)id };
        }

        protected override void MapFieldsToEntity(Airspace entity, object attributes)
        {
            if (attributes is not AirspaceModel attrs) return;

            entity.GlobalId = attrs.GlobalId ?? entity.GlobalId;
            entity.Ident = attrs.Ident;
            entity.IcaoId = attrs.IcaoId;
            entity.Name = attrs.Name;
            entity.TypeCode = attrs.TypeCode;
            entity.LocalType = attrs.LocalType;
            entity.Class = attrs.Class;
            entity.MilCode = attrs.MilCode;
            entity.UpperDesc = attrs.UpperDesc;
            entity.UpperVal = attrs.UpperVal;
            entity.UpperUom = attrs.UpperUom;
            entity.UpperCode = attrs.UpperCode;
            entity.LowerDesc = attrs.LowerDesc;
            entity.LowerVal = attrs.LowerVal;
            entity.LowerUom = attrs.LowerUom;
            entity.LowerCode = attrs.LowerCode;
            entity.Level = attrs.Level;
            entity.Sector = attrs.Sector;
            entity.Onshore = attrs.Onshore;
            entity.Exclusion = attrs.Exclusion;
            entity.WkhrCode = attrs.WkhrCode;
            entity.WkhrRmk = attrs.WkhrRmk;
            entity.Dst = attrs.Dst;
            entity.GmtOffset = attrs.GmtOffset;
            entity.ContAgent = attrs.ContAgent;
            entity.City = attrs.City;
            entity.State = attrs.State;
            entity.Country = attrs.Country;
            entity.AdhpId = attrs.AdhpId;
            entity.UsHigh = attrs.UsHigh;
            entity.AkHigh = attrs.AkHigh;
            entity.AkLow = attrs.AkLow;
            entity.UsLow = attrs.UsLow;
            entity.UsArea = attrs.UsArea;
            entity.Pacific = attrs.Pacific;
            entity.ShapeArea = attrs.ShapeArea;
            entity.ShapeLength = attrs.ShapeLength;
        }
    }
}
