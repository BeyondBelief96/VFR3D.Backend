using Microsoft.EntityFrameworkCore;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data
{
    public class VFR3DDbContext : DbContext
    {
        public VFR3DDbContext(DbContextOptions<VFR3DDbContext> options) : base(options)
        {

        }

        public virtual DbSet<Flight> Flights => Set<Flight>();

        public virtual DbSet<AircraftPerformanceProfile> AircraftPerformanceProfiles => Set<AircraftPerformanceProfile>();

        public virtual DbSet<Aircraft> Aircraft => Set<Aircraft>();

        public virtual DbSet<WeightBalanceProfile> WeightBalanceProfiles => Set<WeightBalanceProfile>();

        public virtual DbSet<WeightBalanceCalculation> WeightBalanceCalculations => Set<WeightBalanceCalculation>();

        public virtual DbSet<AircraftDocument> AircraftDocuments => Set<AircraftDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VFR3DDbContext).Assembly);
        }
    }
}
