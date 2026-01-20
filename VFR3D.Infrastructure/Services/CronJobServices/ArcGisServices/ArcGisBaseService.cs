using NetTopologySuite.Geometries;
using NetTopologySuite;
using System.Text.Json;
using VFR3D.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using VFR3D.Infrastructure.Utilities;
using VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices.Models;

namespace VFR3D.Infrastructure.Services.CronJobServices.ArcGisServices
{
    public abstract class ArcGisBaseService<TEntity> where TEntity : class
    {
        protected readonly ILogger _logger;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly VFR3DDbContext _dbContext;
        protected readonly JsonSerializerOptions _jsonOptions;
        protected readonly GeometryFactory _geometryFactory;
        protected abstract string BaseUrl { get; }

        /// <summary>
        /// Number of records to fetch per page. ArcGIS services typically have a max of 1000-2000.
        /// </summary>
        protected virtual int PageSize => 1000;

        protected ArcGisBaseService(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            VFR3DDbContext dbContext)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;
            _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Query features with automatic pagination to handle large datasets.
        /// </summary>
        protected async Task<ArcGisResponse<TAttributes>> QueryFeatures<TAttributes>(
            Dictionary<string, string> parameters,
            CancellationToken cancellationToken = default)
        {
            var allFeatures = new List<ArcGisFeature<TAttributes>>();
            var offset = 0;
            bool hasMoreRecords;

            do
            {
                var pageResponse = await QueryFeaturesPage<TAttributes>(parameters, offset, cancellationToken);
                allFeatures.AddRange(pageResponse.Features);
                hasMoreRecords = pageResponse.ExceededTransferLimit;
                offset += pageResponse.Features.Count;

                if (hasMoreRecords)
                {
                    _logger.LogDebug("Fetched {Count} features, total so far: {Total}. Fetching more...",
                        pageResponse.Features.Count, allFeatures.Count);
                }
            } while (hasMoreRecords);

            _logger.LogInformation("Total features fetched: {Count}", allFeatures.Count);

            return new ArcGisResponse<TAttributes>
            {
                Features = allFeatures,
                ExceededTransferLimit = false
            };
        }

        /// <summary>
        /// Query a single page of features from ArcGIS.
        /// </summary>
        private async Task<ArcGisResponse<TAttributes>> QueryFeaturesPage<TAttributes>(
            Dictionary<string, string> parameters,
            int offset,
            CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ArcGis");
                var queryParams = new Dictionary<string, string>(parameters)
                {
                    ["f"] = "json",
                    ["outSR"] = "4326",
                    ["resultRecordCount"] = PageSize.ToString(),
                    ["resultOffset"] = offset.ToString()
                };

                var url = WebUtilities.AddQueryString(BaseUrl, queryParams);
                _logger.LogDebug("Fetching ArcGIS features from offset {Offset}", offset);

                var response = await httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<ArcGisResponse<TAttributes>>(content, _jsonOptions);

                if (result?.Features == null)
                {
                    throw new Exception("Invalid response from ArcGIS service");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying ArcGIS features at offset {Offset}", offset);
                throw;
            }
        }

        protected Geometry? CreatePolygonFromRings(List<double[]>[] rings)
        {
            if (rings == null || !rings.Any() || !rings[0].Any())
                return null;

            var coordinates = rings[0]
                .Select(point => new Coordinate(point[0], point[1]))
                .ToArray();

            // Close the ring if not already closed
            if (!coordinates[0].Equals2D(coordinates[^1]))
            {
                coordinates = coordinates.Append(coordinates[0]).ToArray();
            }

            var linearRing = _geometryFactory.CreateLinearRing(coordinates);
            return _geometryFactory.CreatePolygon(linearRing);
        }

        protected abstract Task<TEntity?> FindExistingEntity(object id, CancellationToken cancellationToken);
        protected abstract TEntity CreateNewEntity(object id);
        protected abstract void MapFieldsToEntity(TEntity entity, object attributes);
    }
}
