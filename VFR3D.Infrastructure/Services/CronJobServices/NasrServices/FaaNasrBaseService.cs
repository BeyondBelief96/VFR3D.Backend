using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO.Compression;
using System.Linq.Expressions;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using VFR3D.Infrastructure.Data;
using VFR3D.Infrastructure.Enums;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Utils;
using VFR3D.Infrastructure.Utilities;

namespace VFR3D.Infrastructure.Services.CronJobServices.NasrServices
{
    // Delegate types for compiled expressions
    public delegate string KeyExtractor<in T>(T entity);
    public delegate IQueryable<T> KeyMatcher<T>(IQueryable<T> queryable, List<string> keys);

    public abstract class FaaNasrBaseService<T> where T : class, INasrEntity<T>, new()
    {
        private readonly int _csvBatchSize = 1000;
        private readonly int _dbBatchSize = 100;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VFR3DDbContext _dbContext;
        private readonly IFaaPublicationCycleService _faaPublicationCycleService;
        private readonly string _baseUrl = "https://nfdc.faa.gov/webContent/28DaySub/extra/";

        private readonly KeyExtractor<T> _keyExtractor;
        private readonly KeyMatcher<T> _keyMatcher;
        private readonly Lazy<HashSet<string>> _allPropertyNames;

        protected abstract NasrDataType DataType { get; }
        protected abstract string[] UniqueIdentifiers { get; }
        protected abstract IEnumerable<(string FileName, Type ClassMap, bool IsBaseData)> CsvMappings { get; }
        protected virtual bool UsesLegacySiteNoDeduplication => true;
        protected abstract PublicationType PublicationType { get; }

        protected FaaNasrBaseService(
            ILogger logger,
            IHttpClientFactory httpClientFactory,
            IFaaPublicationCycleService publicationCycleService,
            VFR3DDbContext dbContext)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _faaPublicationCycleService = publicationCycleService;
            _dbContext = dbContext;

            _keyExtractor = CreateKeyExtractor();
            _keyMatcher = CreateKeyMatcher();
            _allPropertyNames = new Lazy<HashSet<string>>(() =>
                typeof(T).GetProperties().Select(p => p.Name).ToHashSet());
        }

        public async Task DownloadAndProcessDataAsync(CancellationToken cancellationToken = default)
        {
            var publicationCycle = await _faaPublicationCycleService.GetPublicationCycleAsync(PublicationType);
            if (publicationCycle == null)
            {
                _logger.LogWarning($"No publication cycle found for {DataType}");
                return;
            }

            var currentPublicationDate = FaaPublicationDateUtils.CalculateCurrentPublicationDate(
                publicationCycle.KnownValidDate,
                publicationCycle.CycleLengthDays);
            var dateString = FaaPublicationDateUtils.FormatDateForNasr(currentPublicationDate);
            var fileName = $"{dateString}_{DataType}_CSV.zip";
            var zipUrl = $"{_baseUrl}{fileName}";

            try
            {
                _logger.LogInformation($"Downloading {DataType} data from {zipUrl}");
                using var client = _httpClientFactory.CreateClient();
                await using var response = await client.GetStreamAsync(zipUrl, cancellationToken);
                using var archive = new ZipArchive(response);

                if (UsesLegacySiteNoDeduplication)
                {
                    await ProcessLegacyMultiFileDatasetAsync(archive, cancellationToken);
                }
                else
                {
                    await ProcessStandaloneDatasetAsync(archive, cancellationToken);
                }

                _logger.LogInformation($"{DataType} data update completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing {DataType} data");
                throw;
            }
        }

        private KeyExtractor<T> CreateKeyExtractor()
        {
            var parameter = Expression.Parameter(typeof(T), "entity");
            Expression body;

            if (UniqueIdentifiers.Length == 1)
            {
                // Single property: just return the property value as string
                var property = Expression.Property(parameter, UniqueIdentifiers[0]);
                var toString = typeof(object).GetMethod(nameof(ToString))!;
                body = Expression.Call(property, toString);
            }
            else
            {
                // Multiple properties: concatenate with "|"
                var stringConcat = typeof(string).GetMethod(nameof(string.Join), new[] { typeof(string), typeof(string[]) })!;
                var properties = UniqueIdentifiers.Select(id =>
                {
                    var prop = Expression.Property(parameter, id);
                    var toString = typeof(object).GetMethod(nameof(ToString))!;
                    return Expression.Call(prop, toString);
                }).ToArray();

                var arrayInit = Expression.NewArrayInit(typeof(string), properties);
                body = Expression.Call(stringConcat, Expression.Constant("|"), arrayInit);
            }

            var lambda = Expression.Lambda<KeyExtractor<T>>(body, parameter);
            return lambda.Compile();
        }

        private KeyMatcher<T> CreateKeyMatcher()
        {
            if (UniqueIdentifiers.Length == 1)
            {
                // Single identifier: use Contains for efficiency
                return (queryable, keys) =>
                {
                    var parameter = Expression.Parameter(typeof(T), "e");
                    var property = Expression.Property(parameter, UniqueIdentifiers[0]);
                    var containsMethod = typeof(List<string>).GetMethod("Contains", new[] { typeof(string) })!;
                    var containsCall = Expression.Call(Expression.Constant(keys), containsMethod, property);
                    var lambda = Expression.Lambda<Func<T, bool>>(containsCall, parameter);
                    return queryable.Where(lambda);
                };
            }
            else
            {
                // Composite keys: need to match each key individually (less efficient but necessary)
                return (queryable, keys) =>
                {
                    var parameter = Expression.Parameter(typeof(T), "e");
                    Expression? combinedCondition = null;

                    foreach (var key in keys)
                    {
                        var keyParts = key.Split('|');
                        if (keyParts.Length != UniqueIdentifiers.Length) continue;

                        Expression? keyCondition = null;
                        for (var i = 0; i < UniqueIdentifiers.Length; i++)
                        {
                            var property = Expression.Property(parameter, UniqueIdentifiers[i]);
                            var equals = Expression.Equal(property, Expression.Constant(keyParts[i]));
                            keyCondition = keyCondition == null ? equals : Expression.AndAlso(keyCondition, equals);
                        }

                        if (keyCondition != null)
                        {
                            combinedCondition = combinedCondition == null ? keyCondition : Expression.OrElse(combinedCondition, keyCondition);
                        }
                    }

                    if (combinedCondition != null)
                    {
                        var lambda = Expression.Lambda<Func<T, bool>>(combinedCondition, parameter);
                        return queryable.Where(lambda);
                    }

                    return queryable.Where(e => false); // No matches
                };
            }
        }

        private async Task ProcessLegacyMultiFileDatasetAsync(ZipArchive archive, CancellationToken cancellationToken)
        {
            // Process base data first
            var baseMapping = CsvMappings.FirstOrDefault(m => m.IsBaseData);
            if (baseMapping != default)
            {
                var baseEntry = archive.Entries.FirstOrDefault(e =>
                    e.Name.Equals(baseMapping.FileName, StringComparison.OrdinalIgnoreCase));

                if (baseEntry != null)
                {
                    await ProcessCsvFileAsync(baseEntry, baseMapping.ClassMap, ProcessingMode.BaseData, null, cancellationToken);
                }
                else
                {
                    _logger.LogWarning($"Base file {baseMapping.FileName} not found in archive");
                }
            }

            // Process supplementary files
            foreach (var mapping in CsvMappings.Where(m => !m.IsBaseData))
            {
                var entry = archive.Entries.FirstOrDefault(e =>
                    e.Name.Equals(mapping.FileName, StringComparison.OrdinalIgnoreCase));

                if (entry != null)
                {
                    var mappedProperties = GetMappedProperties(mapping.ClassMap);
                    await ProcessCsvFileAsync(entry, mapping.ClassMap, ProcessingMode.SupplementaryData, mappedProperties, cancellationToken);
                }
                else
                {
                    _logger.LogWarning($"Supplementary file {mapping.FileName} not found in archive");
                }
            }
        }

        private async Task ProcessStandaloneDatasetAsync(ZipArchive archive, CancellationToken cancellationToken)
        {
            var mapping = CsvMappings.FirstOrDefault();
            if (mapping == default)
            {
                _logger.LogWarning($"No CSV mapping defined for {DataType}");
                return;
            }

            var entry = archive.Entries.FirstOrDefault(e =>
                e.Name.Equals(mapping.FileName, StringComparison.OrdinalIgnoreCase));

            if (entry != null)
            {
                await ProcessCsvFileAsync(entry, mapping.ClassMap, ProcessingMode.Standalone, null, cancellationToken);
            }
            else
            {
                _logger.LogWarning($"File {mapping.FileName} not found in archive");
            }
        }

        private async Task ProcessCsvFileAsync(
            ZipArchiveEntry entry,
            Type classMap,
            ProcessingMode mode,
            HashSet<string>? mappedProperties,
            CancellationToken cancellationToken)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            using var csv = new CsvReader(reader, GetCsvConfiguration());

            ConfigureCsvReader(csv);
            csv.Context.RegisterClassMap(classMap);

            var processedKeys = new HashSet<string>();
            var entities = new List<T>();
            var totalProcessed = 0;

            try
            {
                await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
                {
                    var uniqueKey = _keyExtractor(record);

                    // Skip duplicates within the CSV file
                    if (!processedKeys.Add(uniqueKey))
                    {
                        _logger.LogDebug($"Skipping duplicate record in CSV: {uniqueKey}");
                        continue;
                    }

                    var processedRecord = mode == ProcessingMode.SupplementaryData
                        ? record.CreateSelectiveEntity(mappedProperties!)
                        : record;

                    entities.Add(processedRecord);
                    totalProcessed++;

                    if (entities.Count >= _csvBatchSize)
                    {
                        await ProcessEntitiesBatchAsync(entities, mode, mappedProperties, cancellationToken);
                        entities.Clear();

                        _logger.LogInformation($"Processed {totalProcessed} {DataType} records from {entry.Name}");
                    }
                }

                // Process remaining entities
                if (entities.Count > 0)
                {
                    await ProcessEntitiesBatchAsync(entities, mode, mappedProperties, cancellationToken);
                }

                _logger.LogInformation($"Completed processing {totalProcessed} {DataType} records from {entry.Name}");
            }
            catch (ReaderException ex)
            {
                _logger.LogError(ex, $"CSV parsing error in {entry.Name} at row {ex.Context?.Parser?.Row}");
                throw;
            }
        }

        private async Task ProcessEntitiesBatchAsync(
            List<T> entities,
            ProcessingMode mode,
            HashSet<string>? mappedProperties,
            CancellationToken cancellationToken)
        {
            // Split into smaller batches for database operations
            for (var i = 0; i < entities.Count; i += _dbBatchSize)
            {
                var batch = entities.Skip(i).Take(_dbBatchSize).ToList();
                await ProcessDatabaseBatchAsync(batch, mode, mappedProperties, cancellationToken);
            }
        }

        private async Task ProcessDatabaseBatchAsync(
            List<T> batch,
            ProcessingMode mode,
            HashSet<string>? mappedProperties,
            CancellationToken cancellationToken)
        {
            // Get all unique keys for this batch using compiled expression
            var batchKeys = batch.Select(entity => _keyExtractor(entity)).ToList();

            // Find existing entities in a single query using compiled expression
            var existingEntities = await _keyMatcher(_dbContext.Set<T>(), batchKeys).ToListAsync(cancellationToken);
            var existingLookup = existingEntities.ToDictionary(entity => _keyExtractor(entity));

            foreach (var entity in batch)
            {
                var uniqueKey = _keyExtractor(entity);

                if (existingLookup.TryGetValue(uniqueKey, out var existingEntity))
                {
                    // Use UpdateFrom for ALL update scenarios to avoid PK modification
                    if (mode == ProcessingMode.SupplementaryData)
                    {
                        existingEntity.UpdateFrom(entity, mappedProperties);
                    }
                    else
                    {
                        existingEntity.UpdateFrom(entity); // Use the entity's own UpdateFrom method
                    }
                    
                    _dbContext.Entry(existingEntity).State = EntityState.Modified;
                }
                else
                {
                    // Add new entity
                    _dbContext.Set<T>().Add(entity);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private HashSet<string> GetMappedProperties(Type classMap)
        {
            // This is the only remaining reflection, but it's done once per file, not per record
            var mapInstance = Activator.CreateInstance(classMap) as ClassMap;
            return mapInstance?.MemberMaps
                .Select(m => m.Data.Member?.Name)
                .Where(name => !string.IsNullOrEmpty(name))
                .Cast<string>() // Cast to non-nullable string after filtering out nulls
                .ToHashSet() ?? _allPropertyNames.Value;
        }

        private static CsvConfiguration GetCsvConfiguration()
        {
            return new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                IgnoreBlankLines = true,
                DetectDelimiter = true
            };
        }

        private static void ConfigureCsvReader(CsvReader csv)
        {
            csv.Context.TypeConverterCache.RemoveConverter<decimal?>();
            csv.Context.TypeConverterCache.RemoveConverter<int?>();
            csv.Context.TypeConverterCache.AddConverter<decimal?>(new OptionalDecimalConverter());
            csv.Context.TypeConverterCache.AddConverter<int?>(new OptionalIntConverter());
            csv.Context.TypeConverterCache.AddConverter<DateTime?>(new OptionalDateConverter());
        }

        private enum ProcessingMode
        {
            BaseData,
            SupplementaryData,
            Standalone
        }
    }
}