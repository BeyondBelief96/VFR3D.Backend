using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VFR3D.Domain.Entities;
using VFR3D.Domain.ValueObjects.FaaPublications;

namespace VFR3D.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(VFR3DDbContext context, ILogger logger)
        {
            await InitializeFaaPublicationCyclesAsync(context, logger);
        }

        private static async Task InitializeFaaPublicationCyclesAsync(VFR3DDbContext context, ILogger logger)
        {
            logger.LogInformation("Checking FAA Publication Cycle data...");
            
            // Define all expected records
            var expectedRecords = new Dictionary<PublicationType, (int Id, int CycleLengthDays, DateTime KnownValidDate)>
            {
                { PublicationType.ChartSupplement, (1, 56, new DateTime(2024, 12, 26, 0, 0, 0, DateTimeKind.Utc)) },
                { PublicationType.AirportDiagram, (2, 28, new DateTime(2024, 12, 26, 0, 0, 0, DateTimeKind.Utc)) },
                { PublicationType.NasrSubscription_Airport, (3, 28, new DateTime(2025, 1, 23, 0, 0, 0, DateTimeKind.Utc)) },
                { PublicationType.NasrSubscription_Frequencies, (4, 28, new DateTime(2025, 1, 23, 0, 0, 0, DateTimeKind.Utc)) },
                { PublicationType.Airspaces, (5, 56, new DateTime(2024, 12, 26, 0, 0, 0, DateTimeKind.Utc)) },
                { PublicationType.SpecialUseAirspaces, (6, 56, new DateTime(2024, 12, 26, 0, 0, 0, DateTimeKind.Utc)) }
            };
            
            // Get existing records
            var existingRecords = await context.FaaPublicationCycles.ToDictionaryAsync(c => c.PublicationType);
            
            bool hasChanges = false;
            
            // Process each expected record
            foreach (var expected in expectedRecords)
            {
                var publicationType = expected.Key;
                var (id, cycleLengthDays, knownValidDate) = expected.Value;
                
                if (existingRecords.TryGetValue(publicationType, out var existingRecord))
                {
                    // Record exists - check if it needs updates
                    bool needsUpdate = false;
                    
                    if (existingRecord.Id != id)
                    {
                        existingRecord.Id = id;
                        needsUpdate = true;
                    }
                    
                    if (existingRecord.CycleLengthDays != cycleLengthDays)
                    {
                        existingRecord.CycleLengthDays = cycleLengthDays;
                        needsUpdate = true;
                    }
                    
                    if (existingRecord.KnownValidDate != knownValidDate)
                    {
                        existingRecord.KnownValidDate = knownValidDate;
                        needsUpdate = true;
                    }
                    
                    if (needsUpdate)
                    {
                        context.FaaPublicationCycles.Update(existingRecord);
                        hasChanges = true;
                        logger.LogInformation($"Updated FAA Publication Cycle record for {publicationType}");
                    }
                }
                else
                {
                    // Record doesn't exist - add it
                    var newRecord = new FaaPublicationCycle
                    {
                        Id = id,
                        PublicationType = publicationType,
                        CycleLengthDays = cycleLengthDays,
                        KnownValidDate = knownValidDate
                    };
                    
                    await context.FaaPublicationCycles.AddAsync(newRecord);
                    hasChanges = true;
                    logger.LogInformation($"Added new FAA Publication Cycle record for {publicationType}");
                }
            }
            
            if (hasChanges)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("FAA Publication Cycle data updated successfully");
            }
            else
            {
                logger.LogInformation("FAA Publication Cycle data is up to date");
            }
        }
    }
}
