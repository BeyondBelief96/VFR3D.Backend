using Microsoft.EntityFrameworkCore;
using Vfr3d.Domain.Entities;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data
{
    public class CronServiceDbContext : DbContext
    {
        public CronServiceDbContext(DbContextOptions<CronServiceDbContext> options) : base(options)
        {
            
        }

        public DbSet<Metar> Metars => Set<Metar>();

        public DbSet<Taf> Tafs => Set<Taf>();

        public DbSet<Pirep> Pireps => Set<Pirep>();

        public DbSet<Airsigmet> Airsigmets => Set<Airsigmet>();

        public DbSet<ChartSupplement> ChartSupplements => Set<ChartSupplement>();

        public DbSet<AirportDiagram> AirportDiagrams => Set<AirportDiagram>();

        public DbSet<Airport> Airports => Set<Airport>();

        public DbSet<CommunicationFrequency> CommunicationFrequency => Set<CommunicationFrequency>();

        public DbSet<FaaPublicationCycle> FaaPublicationCycles => Set<FaaPublicationCycle>();   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CronServiceDbContext).Assembly);
        }
    }
}
