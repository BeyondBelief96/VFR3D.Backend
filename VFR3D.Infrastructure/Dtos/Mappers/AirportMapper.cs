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
                SiteTypeCode = airport.SiteTypeCode,
                City = airport.City,
                StateCode = airport.StateCode,
                CountryCode = airport.CountryCode,
                StateName = airport.StateName,
                LatDecimal = airport.LatDecimal,
                LongDecimal = airport.LongDecimal,
                Elev = airport.Elev,
                MagVarn = airport.MagVarn,
                MagHemis = airport.MagHemis,
                ChartName = airport.ChartName,
                ArptStatus = airport.ArptStatus,
                FuelTypes = airport.FuelTypes,
                LastInspection = airport.LastInspection,
                LastInfoResponse = airport.LastInfoResponse,
                CustomsFlag = airport.CustomsFlag,
                MilLndgFlag = airport.MilLndgFlag,
                JointUseFlag = airport.JointUseFlag,
                LndgRightsFlag = airport.LndgRightsFlag,
                ContactName = airport.ContactName,
                ContactPhoneNumber = airport.ContactPhoneNumber,
                ContactAddress1 = airport.ContactAddress1,
                ContactAddress2 = airport.ContactAddress2,
                ContactCity = airport.ContactCity,
                ContactState = airport.ContactState,
                ContactTitle = airport.ContactTitle,
                ContactZipCode = airport.ContactZipCode,
                ContactZipPlusFour = airport.ContactZipPlusFour,
            };
        }
    }
}