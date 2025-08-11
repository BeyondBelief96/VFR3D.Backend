using NetTopologySuite.Geometries;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers
{
    public static class AirspaceMapper
    {
        public static AirspaceDto ToDto(Airspace airspace)
        {
            return new AirspaceDto
            {
                GlobalId = airspace.GlobalId,
                Ident = airspace.Ident,
                IcaoId = airspace.IcaoId,
                Name = airspace.Name,
                UpperDesc = airspace.UpperDesc,
                UpperVal = airspace.UpperVal,
                UpperUom = airspace.UpperUom,
                UpperCode = airspace.UpperCode,
                LowerDesc = airspace.LowerDesc,
                LowerVal = airspace.LowerVal,
                LowerUom = airspace.LowerUom,
                LowerCode = airspace.LowerCode,
                TypeCode = airspace.TypeCode,
                LocalType = airspace.LocalType,
                Class = airspace.Class,
                MilCode = airspace.MilCode,
                CommName = airspace.CommName,
                Level = airspace.Level,
                Sector = airspace.Sector,
                Onshore = airspace.Onshore,
                Exclusion = airspace.Exclusion,
                WkhrCode = airspace.WkhrCode,
                WkhrRmk = airspace.WkhrRmk,
                Dst = airspace.Dst,
                GmtOffset = airspace.GmtOffset,
                ContAgent = airspace.ContAgent,
                City = airspace.City,
                State = airspace.State,
                Country = airspace.Country,
                AdhpId = airspace.AdhpId,
                Geometry = airspace.Geometry != null ? ConvertToGeoJson(airspace.Geometry) : null
            };
        }

        public static SpecialUseAirspaceDto ToDto(SpecialUseAirspace airspace)
        {
            return new SpecialUseAirspaceDto
            {
                GlobalId = airspace.GlobalId,
                Name = airspace.Name,
                TypeCode = airspace.TypeCode,
                Class = airspace.Class,
                UpperDesc = airspace.UpperDesc,
                UpperVal = airspace.UpperVal,
                UpperUom = airspace.UpperUom,
                UpperCode = airspace.UpperCode,
                LowerDesc = airspace.LowerDesc,
                LowerVal = airspace.LowerVal,
                LowerUom = airspace.LowerUom,
                LowerCode = airspace.LowerCode,
                LevelCode = airspace.LevelCode,
                City = airspace.City,
                State = airspace.State,
                Country = airspace.Country,
                ContAgent = airspace.ContAgent,
                CommName = airspace.CommName,
                Sector = airspace.Sector,
                Onshore = airspace.Onshore,
                Exclusion = airspace.Exclusion,
                TimesOfUse = airspace.TimesOfUse,
                GmtOffset = airspace.GmtOffset,
                DstCode = airspace.DstCode,
                Remarks = airspace.Remarks,
                Geometry = airspace.Geometry != null ? ConvertToGeoJson(airspace.Geometry) : null
            };
        }

        private static GeoJsonGeometry ConvertToGeoJson(Geometry geometry)
        {
            if (geometry is not Polygon polygon)
            {
                return new GeoJsonGeometry
                {
                    Type = geometry.GeometryType,
                    Coordinates = []
                };
            }

            var exteriorRing = polygon.ExteriorRing.Coordinates
                .Select(c => new[] { c.X, c.Y })
                .ToArray();
            
            var interiorRings = polygon.InteriorRings.Select(ring =>
                ring.Coordinates.Select(c => new[] { c.X, c.Y }).ToArray()
            ).ToArray();

            var allRings = new[] { exteriorRing }.Concat(interiorRings).ToArray();

            return new GeoJsonGeometry
            {
                Type = "Polygon",
                Coordinates = allRings
            };
        }
    }
}