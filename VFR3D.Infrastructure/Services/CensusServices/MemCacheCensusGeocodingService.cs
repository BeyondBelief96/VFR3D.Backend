using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Dtos;
using VFR3D.Infrastructure.Interfaces;

namespace VFR3D.Infrastructure.Services.CensusServices
{
    public class MemCacheCensusGeocodingService : ICensusGeocodingService
    {
        private readonly ICensusGeocodingService _censusService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemCacheCensusGeocodingService> _logger;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(30);

        public MemCacheCensusGeocodingService(
            ICensusGeocodingService censusService,
            IMemoryCache cache,
            ILogger<MemCacheCensusGeocodingService> logger)
        {
            _censusService = censusService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<StateInfoDto?> GetStateFromCoordinates(double latitude, double longitude)
        {
            var cacheKey = $"state_{latitude:F4}_{longitude:F4}";

            if (_cache.TryGetValue<StateInfoDto>(cacheKey, out var cachedState))
            {
                return cachedState;
            }

            var stateInfo = await _censusService.GetStateFromCoordinates(latitude, longitude);

            if (stateInfo != null)
            {
                _cache.Set(cacheKey, stateInfo, CacheDuration);
            }

            return stateInfo;
        }
    }
}
