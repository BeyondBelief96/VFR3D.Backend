using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using VFR3D.Infrastructure.Data;
using CsvHelper;
using System.Globalization;
using VFR3D.Infrastructure.Utilities;
using VFR3D.Domain.ValueObjects.FaaPublications;
using CsvHelper.Configuration;
using VFR3D.Infrastructure.Enums;
using VFR3D.Infrastructure.Interfaces;
using VFR3D.Infrastructure.Services.CronJobServices.NasrServices.Utils;

namespace VFR3D.Infrastructure.Services.CronJobServices.NasrServices
{
    public abstract class FaaNasrBaseService<T> where T : class
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly VFR3DDbContext _dbContext;
        private readonly IFaaPublicationCycleService _faaPublicationCycleService;
        private readonly string _baseUrl = "https://nfdc.faa.gov/webContent/28DaySub/extra/";

        protected abstract NasrDataType DataType { get; }
        protected abstract string[] UniqueIdentifiers { get; }
        protected abstract IEnumerable<(string FileName, Type ClassMap, bool IsBaseData)> CsvMappings { get; }

        // Property to determine if this is a legacy SiteNo-based dataset (like APT)
        // or a standalone dataset with composite keys (like FRQ)
        protected virtual bool UsesLegacySiteNoDeduplication => true;

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
        }

        public async Task DownloadAndProcessDataAsync(CancellationToken cancellationToken = default)
        {
            var publicationCycle = await _faaPublicationCycleService.GetPublicationCycleAsync(PublicationType.NasrSubscription);
            if (publicationCycle != null)
            {
                var currentPublicationDate = FaaPublicationDateUtils.CalculateCurrentPublicationDate(publicationCycle.KnownValidDate, publicationCycle.CycleLengthDays);
                var dateString = FaaPublicationDateUtils.FormatDateForNasr(currentPublicationDate);
                var fileName = $"{dateString}_{DataType}_CSV.zip";
                var zipUrl = $"{_baseUrl}{fileName}";

                try
                {
                    _logger.LogInformation($"Downloading {DataType} data from {zipUrl}");
                    using var client = _httpClientFactory.CreateClient();
                    await using var response = await client.GetStreamAsync(zipUrl, cancellationToken);
                    using var archive = new ZipArchive(response);

                    // Check if this is a legacy multi-file dataset (like APT) or standalone (like FRQ)
                    if (UsesLegacySiteNoDeduplication)
                    {
                        await ProcessLegacyMultiFileDataset(archive, cancellationToken);
                    }
                    else
                    {
                        await ProcessStandaloneDataset(archive, cancellationToken);
                    }

                    _logger.LogInformation($"{DataType} data update completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing {DataType} data");
                    throw;
                }
            }
        }

        private async Task ProcessLegacyMultiFileDataset(ZipArchive archive, CancellationToken cancellationToken)
        {
            // Process base data first
            var baseMapping = CsvMappings.FirstOrDefault(m => m.IsBaseData);
            if (baseMapping != default)
            {
                var baseEntry = archive.Entries.FirstOrDefault(e =>
                    e.Name.Equals(baseMapping.FileName, StringComparison.OrdinalIgnoreCase));

                if (baseEntry != null)
                {
                    await ProcessBaseCsvFileAsync(baseEntry, baseMapping.ClassMap, cancellationToken);
                }
            }

            // Process supplementary files
            foreach (var mapping in CsvMappings.Where(m => !m.IsBaseData))
            {
                var entry = archive.Entries.FirstOrDefault(e =>
                    e.Name.Equals(mapping.FileName, StringComparison.OrdinalIgnoreCase));

                if (entry != null)
                {
                    await ProcessSupplementaryCsvFileAsync(entry, mapping.ClassMap, cancellationToken);
                }
                else
                {
                    _logger.LogWarning($"File {mapping.FileName} not found in archive");
                }
            }
        }

        private async Task ProcessStandaloneDataset(ZipArchive archive, CancellationToken cancellationToken)
        {
            // For standalone datasets, there should be only one CSV mapping
            var mapping = CsvMappings.FirstOrDefault();
            if (mapping != default)
            {
                var entry = archive.Entries.FirstOrDefault(e =>
                    e.Name.Equals(mapping.FileName, StringComparison.OrdinalIgnoreCase));

                if (entry != null)
                {
                    await ProcessStandaloneCsvFileAsync(entry, mapping.ClassMap, cancellationToken);
                }
                else
                {
                    _logger.LogWarning($"File {mapping.FileName} not found in archive");
                }
            }
        }

        private async Task ProcessBaseCsvFileAsync(ZipArchiveEntry entry, Type classMap, CancellationToken cancellationToken)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var config = GetCsvConfiguration();
            using var csv = new CsvReader(reader, config);

            ConfigureCsvReader(csv);
            csv.Context.RegisterClassMap(classMap);

            try
            {
                var entities = new List<T>();
                var processedSiteNos = new HashSet<string>();

                await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
                {
                    // Get the SiteNo property value for legacy deduplication
                    var siteNoProperty = typeof(T).GetProperty("SiteNo");
                    if (siteNoProperty != null)
                    {
                        var siteNo = siteNoProperty.GetValue(record)?.ToString();

                        // Skip if we've already seen this SiteNo
                        if (!string.IsNullOrEmpty(siteNo) && !processedSiteNos.Add(siteNo))
                        {
                            _logger.LogWarning($"Skipping duplicate SiteNo {siteNo} in CSV file");
                            continue;
                        }
                    }

                    entities.Add(record);
                    if (entities.Count >= 1000)
                    {
                        await SaveOrUpdateEntitiesAsync(entities, cancellationToken);
                        entities.Clear();
                    }
                }
                if (entities.Count != 0)
                {
                    await SaveOrUpdateEntitiesAsync(entities, cancellationToken);
                }
            }
            catch (ReaderException ex)
            {
                _logger.LogError($"CSV parsing error at row {ex.Context?.Parser?.Row}, field {ex.Context?.Parser?.RawRecord}");
                throw;
            }
        }

        private async Task ProcessStandaloneCsvFileAsync(ZipArchiveEntry entry, Type classMap, CancellationToken cancellationToken)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var config = GetCsvConfiguration();
            using var csv = new CsvReader(reader, config);

            ConfigureCsvReader(csv);
            csv.Context.RegisterClassMap(classMap);

            try
            {
                var entities = new List<T>();
                var processedRecords = new HashSet<string>();

                await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
                {
                    // Create a unique key based on the composite key fields
                    var uniqueKey = CreateUniqueKey(record);

                    // Skip if we've already seen this record
                    if (!processedRecords.Add(uniqueKey))
                    {
                        _logger.LogWarning($"Skipping duplicate record: {uniqueKey}");
                        continue;
                    }

                    entities.Add(record);
                    if (entities.Count >= 1000)
                    {
                        await SaveOrUpdateEntitiesAsync(entities, cancellationToken);
                        entities.Clear();
                    }
                }
                if (entities.Count != 0)
                {
                    await SaveOrUpdateEntitiesAsync(entities, cancellationToken);
                }
            }
            catch (ReaderException ex)
            {
                _logger.LogError($"CSV parsing error at row {ex.Context?.Parser?.Row}, field {ex.Context?.Parser?.RawRecord}");
                throw;
            }
        }

        private async Task ProcessSupplementaryCsvFileAsync(ZipArchiveEntry entry, Type classMap, CancellationToken cancellationToken)
        {
            await using var entryStream = entry.Open();
            using var reader = new StreamReader(entryStream);
            var config = GetCsvConfiguration();
            using var csv = new CsvReader(reader, config);

            ConfigureCsvReader(csv);

            // Get the mapped properties from the class map
            var mapInstance = Activator.CreateInstance(classMap) as ClassMap;
            var mappedProperties = mapInstance?.MemberMaps.Select(m => m.Data.Member?.Name).ToHashSet() ?? new HashSet<string?>();

            csv.Context.RegisterClassMap(classMap);

            try
            {
                var entities = new List<T>();
                var processedSiteNos = new HashSet<string>();

                await foreach (var record in csv.GetRecordsAsync<T>(cancellationToken))
                {
                    // Create a new instance with only mapped properties
                    var selectiveRecord = Activator.CreateInstance<T>();
                    foreach (var property in typeof(T).GetProperties())
                    {
                        if (mappedProperties.Contains(property.Name))
                        {
                            property.SetValue(selectiveRecord, property.GetValue(record));
                        }
                    }

                    // Rest of your existing code, but use selectiveRecord instead of record
                    var siteNoProperty = typeof(T).GetProperty("SiteNo");
                    if (siteNoProperty != null)
                    {
                        var siteNo = siteNoProperty.GetValue(selectiveRecord)?.ToString();

                        if (!string.IsNullOrEmpty(siteNo) && !processedSiteNos.Add(siteNo))
                        {
                            _logger.LogWarning($"Skipping duplicate SiteNo {siteNo} in CSV file");
                            continue;
                        }
                    }

                    var existingEntity = await FindExistingEntityAsync(selectiveRecord, cancellationToken);
                    if (existingEntity != null)
                    {
                        foreach (var property in typeof(T).GetProperties())
                        {
                            if (mappedProperties.Contains(property.Name))
                            {
                                var value = property.GetValue(selectiveRecord);
                                property.SetValue(existingEntity, value);
                            }
                        }

                        entities.Add(existingEntity);
                        if (entities.Count >= 1000)
                        {
                            await SaveOrUpdateEntitiesAsync(entities, cancellationToken, true);
                            entities.Clear();
                        }
                    }
                }
                if (entities.Count != 0)
                {
                    await SaveOrUpdateEntitiesAsync(entities, cancellationToken, true);
                }
            }
            catch (ReaderException ex)
            {
                _logger.LogError($"CSV parsing error at row {ex.Context?.Parser?.Row}, field {ex.Context?.Parser?.RawRecord}");
                throw;
            }
        }

        private string CreateUniqueKey(T entity)
        {
            var keyParts = new List<string>();

            foreach (var identifier in UniqueIdentifiers)
            {
                var propertyInfo = typeof(T).GetProperty(identifier);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(entity)?.ToString() ?? string.Empty;
                    keyParts.Add(value);
                }
            }

            return string.Join("|", keyParts);
        }

        private CsvConfiguration GetCsvConfiguration()
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

        private void ConfigureCsvReader(CsvReader csv)
        {
            csv.Context.TypeConverterCache.RemoveConverter<decimal?>();
            csv.Context.TypeConverterCache.RemoveConverter<int?>();
            csv.Context.TypeConverterCache.AddConverter<decimal?>(new OptionalDecimalConverter());
            csv.Context.TypeConverterCache.AddConverter<int?>(new OptionalIntConverter());
            csv.Context.TypeConverterCache.AddConverter<DateTime?>(new OptionalDateConverter());
        }

        private async Task SaveOrUpdateEntitiesAsync(IEnumerable<T> entities, CancellationToken cancellationToken, bool isSupplementaryData = false)
        {
            const int batchSize = 100;
            var entitiesList = entities.ToList();

            for (var i = 0; i < entitiesList.Count; i += batchSize)
            {
                var batch = entitiesList.Skip(i).Take(batchSize);

                foreach (var entity in batch)
                {
                    var existingEntity = await FindExistingEntityAsync(entity, cancellationToken);
                    if (existingEntity != null)
                    {
                        if (isSupplementaryData && UsesLegacySiteNoDeduplication)
                        {
                            // For supplementary data in legacy datasets, only update non-null values
                            foreach (var property in typeof(T).GetProperties())
                            {
                                var value = property.GetValue(entity);
                                if (value != null)
                                {
                                    property.SetValue(existingEntity, value);
                                }
                            }
                            _dbContext.Update(existingEntity);
                        }
                        else
                        {
                            // For base data or standalone datasets, update all values
                            _dbContext.Entry(existingEntity).CurrentValues.SetValues(entity);
                        }
                    }
                    else
                    {
                        await _dbContext.Set<T>().AddAsync(entity, cancellationToken);
                    }
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation($"Processed batch {i + 1} to {Math.Min(i + batchSize, entitiesList.Count)} of {entitiesList.Count} {DataType} entities");
            }
        }

        private async Task<T?> FindExistingEntityAsync(T entity, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Set<T>().AsQueryable();

            foreach (var identifier in UniqueIdentifiers)
            {
                var propertyInfo = typeof(T).GetProperty(identifier);
                if (propertyInfo == null) continue;

                var value = propertyInfo.GetValue(entity);
                queryable = queryable.Where(e => EF.Property<object>(e, identifier) == value);
            }

            return await queryable.FirstOrDefaultAsync(cancellationToken);
        }
    }
}