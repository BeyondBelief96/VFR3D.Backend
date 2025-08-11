using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.ArcgisServices.Models;

namespace VFR3D.Infrastructure.Services.ArcgisServices
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

            foreach (var feature in response.Features)
            {
                var existingAirspace = await _dbContext.SpecialUseAirspaces.FirstOrDefaultAsync(a => a.GlobalId == feature.Attributes.GlobalId, cancellationToken)
                    ?? new SpecialUseAirspace { GlobalId = feature.Attributes.GlobalId ?? string.Empty };

                MapFieldsToEntity(existingAirspace, feature.Attributes);
                existingAirspace.Geometry = CreatePolygonFromRings(feature.Geometry?.Rings ?? Array.Empty<List<double[]>>());

                _dbContext.SpecialUseAirspaces.Update(existingAirspace);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated {Count} special use airspaces", response.Features.Count);
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