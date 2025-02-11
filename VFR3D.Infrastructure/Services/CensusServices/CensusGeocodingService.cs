using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CensusServices.Models;

namespace VFR3D.Infrastructure.Services.CensusServices
{

    public class CensusGeocodingService : ICensusGeocodingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CensusGeocodingService> _logger;
        private const string BaseUrl = "https://geocoding.geo.census.gov/geocoder/geographies/coordinates";

        public CensusGeocodingService(IHttpClientFactory httpClientFactory, ILogger<CensusGeocodingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<StateInfoDto?> GetStateFromCoordinates(double latitude, double longitude)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var url = $"{BaseUrl}?x={longitude}&y={latitude}&benchmark=Public_AR_Current&vintage=Current_Current&format=json&layers=States";
                var response = await httpClient.GetFromJsonAsync<CensusResponse>(url);

                var stateInfo = response?.Result?.GeographicData?.FirstOrDefault();
                if (stateInfo != null)
                {
                    var stateCode = StateCodeMapper.GetStateCode(stateInfo.State.Fips);
                    if (stateCode != null)
                    {
                        return new StateInfoDto
                        {
                            StateCode = stateCode,
                            StateName = stateInfo.State.Name
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting state info for coordinates: {Latitude}, {Longitude}", latitude, longitude);
                throw;
            }
        }
    }
}
