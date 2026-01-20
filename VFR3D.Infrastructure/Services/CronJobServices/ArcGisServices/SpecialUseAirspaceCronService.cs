using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices.Models;

namespace VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices
{
    public class SpecialUseAirspaceCronService : ArcGisBaseService<SpecialUseAirspace>, IAirspaceCronService<SpecialUseAirspace>
    {
        protected override string BaseUrl => "https://services6.arcgis.com/ssFJjBXIUyZDrSYZ/arcgis/rest/services/Special_Use_Airspace/FeatureServer/0/query";

        public SpecialUseAirspaceCronService(
            ILogger<SpecialUseAirspaceCronService> logger,
            IHttpClientFactory httpClientFactory,
            VFR3DDbContext dbContext)
            : base(logger, httpClientFactory, dbContext)
        {
        }

        public async Task UpdateAirspacesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating special use airspaces");

            var parameters = new Dictionary<string, string>
            {
                ["where"] = "1=1",
                ["outFields"] = "*"
            };

            var response = await QueryFeatures<SpecialUseAirspaceModel>(parameters, cancellationToken);

            // Extract valid GlobalIds from response
            var globalIds = response.Features
                .Select(f => f.Attributes.GlobalId)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            // Batch load existing airspaces to avoid N+1 queries
            var existingAirspaces = await _dbContext.SpecialUseAirspaces
                .Where(a => globalIds.Contains(a.GlobalId))
                .ToDictionaryAsync(a => a.GlobalId, cancellationToken);

            var newAirspaces = new List<SpecialUseAirspace>();
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
                    var newAirspace = new SpecialUseAirspace { GlobalId = globalId };
                    MapFieldsToEntity(newAirspace, feature.Attributes);
                    newAirspace.Geometry = CreatePolygonFromRings(feature.Geometry?.Rings ?? Array.Empty<List<double[]>>());
                    newAirspaces.Add(newAirspace);
                }
            }

            if (newAirspaces.Count > 0)
            {
                await _dbContext.SpecialUseAirspaces.AddRangeAsync(newAirspaces, cancellationToken);
                _logger.LogDebug("Adding {Count} new special use airspaces", newAirspaces.Count);
            }

            if (skippedCount > 0)
            {
                _logger.LogWarning("Skipped {Count} special use airspaces with null or empty GlobalId", skippedCount);
            }

            try
            {
                var changesCount = await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Processed {Count} special use airspaces: {NewCount} new, {UpdatedCount} updated, {Changes} database changes",
                    response.Features.Count, newAirspaces.Count, existingAirspaces.Count, changesCount);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency exception while updating special use airspaces. This may indicate the data was modified by another process.");

                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is SpecialUseAirspace airspace)
                    {
                        _logger.LogWarning("Concurrency conflict for special use airspace GlobalId: {GlobalId}", airspace.GlobalId);
                        await entry.ReloadAsync(cancellationToken);
                    }
                }

                throw;
            }
        }

        protected override async Task<SpecialUseAirspace?> FindExistingEntity(object id, CancellationToken cancellationToken)
        {
            return await _dbContext.SpecialUseAirspaces.FirstOrDefaultAsync(a => a.GlobalId == (string)id, cancellationToken);
        }

        protected override SpecialUseAirspace CreateNewEntity(object id)
        {
            return new SpecialUseAirspace { GlobalId = (string)id };
        }

        protected override void MapFieldsToEntity(SpecialUseAirspace entity, object attributes)
        {
            if (attributes is not SpecialUseAirspaceModel attrs) return;

            entity.GlobalId = attrs.GlobalId ?? entity.GlobalId;
            entity.Name = attrs.Name;
            entity.TypeCode = attrs.TypeCode;
            entity.Class = attrs.Class;
            entity.UpperDesc = attrs.UpperDesc;
            entity.UpperVal = attrs.UpperVal;
            entity.UpperUom = attrs.UpperUom;
            entity.UpperCode = attrs.UpperCode;
            entity.LowerDesc = attrs.LowerDesc;
            entity.LowerVal = attrs.LowerVal;
            entity.LowerUom = attrs.LowerUom;
            entity.LowerCode = attrs.LowerCode;
            entity.City = attrs.City;
            entity.State = attrs.State;
            entity.Country = attrs.Country;
            entity.ContAgent = attrs.ContAgent;
            entity.Sector = attrs.Sector;
            entity.Onshore = attrs.Onshore;
            entity.Exclusion = attrs.Exclusion;
            entity.TimesOfUse = attrs.TimesOfUse;
            entity.GmtOffset = attrs.GmtOffset;
            entity.Remarks = attrs.Remarks;
            entity.AkLow = attrs.AkLow;
            entity.AkHigh = attrs.AkHigh;
            entity.UsLow = attrs.UsLow;
            entity.UsHigh = attrs.UsHigh;
            entity.UsArea = attrs.UsArea;
            entity.Pacific = attrs.Pacific;
            entity.ShapeArea = attrs.ShapeArea;
            entity.ShapeLength = attrs.ShapeLength;
        }
    }
}