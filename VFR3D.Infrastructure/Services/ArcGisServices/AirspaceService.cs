using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.ArcgisServices.Models;

namespace VFR3D.Infrastructure.Services.ArcgisServices
{
    public class AirspaceService : ArcGisBaseService<Airspace>, IAirspaceService<Airspace>
    {
        protected override string BaseUrl => "https://services6.arcgis.com/ssFJjBXIUyZDrSYZ/arcgis/rest/services/Class_Airspace/FeatureServer/0/query";

        public AirspaceService(
            ILogger<AirspaceService> logger,
            IHttpClientFactory httpClientFactory,
            CronServiceDbContext dbContext)
            : base(logger, httpClientFactory, dbContext)
        {
        }

        public async Task UpdateAirspacesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var airspaceClass in new[] { "B", "C", "D", "E" })
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

            foreach (var feature in response.Features)
            {
                var existingAirspace = await FindExistingEntity(feature.Attributes.ObjectId, cancellationToken)
                    ?? CreateNewEntity(feature.Attributes.ObjectId);

                MapFieldsToEntity(existingAirspace, feature.Attributes);
                existingAirspace.Geometry = CreatePolygonFromRings(feature.Geometry?.Rings ?? Array.Empty<List<double[]>>());

                if (existingAirspace.Id == 0)
                {
                    await _dbContext.Airspaces.AddAsync(existingAirspace, cancellationToken);
                }
                else
                {
                    _dbContext.Airspaces.Update(existingAirspace);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Updated {Count} Class {Class} airspaces", response.Features.Count, airspaceClass);
        }

        protected override async Task<Airspace?> FindExistingEntity(object id, CancellationToken cancellationToken)
        {
            return await _dbContext.Airspaces.FirstOrDefaultAsync(a => a.ObjectId == (int)id, cancellationToken);
        }

        protected override Airspace CreateNewEntity(object id)
        {
            return new Airspace { ObjectId = (int)id };
        }

        protected override void MapFieldsToEntity(Airspace entity, object attributes)
        {
            if (attributes is not AirspaceModel attrs) return;

            entity.GlobalId = attrs.GlobalId;
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
