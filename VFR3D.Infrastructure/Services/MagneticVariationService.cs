using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VFR3D.Infrastructure.Dtos.Navlog;
using VFR3D.Infrastructure.Settings;

namespace VFR3D.Infrastructure.Services;

public class MagneticVariationService : IMagneticVariationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly ILogger<MagneticVariationService> _logger;
    private readonly MemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public MagneticVariationService(
        IHttpClientFactory httpClientFactory,
        IOptions<ApiKeys> apiKeys,
        ILogger<MagneticVariationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _apiKey = apiKeys.Value.NOAAApiKey ?? throw new ArgumentNullException(nameof(apiKeys));
        _logger = logger;
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<double> GetMagneticVariation(double latitude, double longitude)
    {
        try
        {
            string cacheKey = $"{latitude},{longitude}";
            
            if (_cache.TryGetValue(cacheKey, out double cachedVariation))
            {
                return cachedVariation;
            }
            
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(
                $"https://www.ngdc.noaa.gov/geomag-web/calculators/calculateDeclination?" +
                $"lat1={latitude}&lon1={longitude}&key={_apiKey}&resultFormat=json");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<MagneticVariationResponseDto>(content);

            if (result?.Result == null || result.Result.Length == 0)
            {
                throw new InvalidOperationException("Invalid response from NOAA API");
            }

            var declination = result.Result[0].Declination;
            
            // Cache the result
            _cache.Set(cacheKey, declination, CacheDuration);

            return declination;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting magnetic variation for lat: {Latitude}, lon: {Longitude}", 
                latitude, longitude);
            throw;
        }
    }
}


