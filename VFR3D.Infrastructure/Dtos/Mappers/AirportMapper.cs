using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Dtos.Mappers
{
    public static class AirportMapper
    {
        public static AirportDto ToDto(Airport airport)
        {
            return new AirportDto
            {
                SiteNo = airport.SiteNo,
                IcaoId = airport.IcaoId,
                ArptId = airport.ArptId,
                ArptName = airport.ArptName,
                City = airport.City,
                StateCode = airport.StateCode,
                StateName = airport.StateName,
                LatDecimal = airport.LatDecimal,
                LongDecimal = airport.LongDecimal,
                Elev = airport.Elev,
                ChartName = airport.ChartName,
                ArptStatus = airport.ArptStatus,
                FuelTypes = airport.FuelTypes,
                LastInspection = airport.LastInspection,
                LastInfoResponse = airport.LastInfoResponse,
                ContactName = airport.ContactName,
                ContactPhoneNumber = airport.ContactPhoneNumber
            };
        }
    }
}