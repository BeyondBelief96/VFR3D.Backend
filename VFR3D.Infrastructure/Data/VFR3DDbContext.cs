using Microsoft.EntityFrameworkCore;
using Vfr3d.Domain.Entities;
using VFR3D.Domain.Entities;

namespace VFR3D.Infrastructure.Data
{
    public class VFR3DDbContext : DbContext
    {
        public VFR3DDbContext(DbContextOptions<VFR3DDbContext> options) : base(options)
        {
            
        }

        public virtual DbSet<Metar> Metars => Set<Metar>();

        public virtual DbSet<Taf> Tafs => Set<Taf>();

        public virtual DbSet<Pirep> Pireps => Set<Pirep>();

        public virtual DbSet<Airsigmet> Airsigmets => Set<Airsigmet>();

        public virtual DbSet<ChartSupplement> ChartSupplements => Set<ChartSupplement>();

        public virtual DbSet<AirportDiagram> AirportDiagrams => Set<AirportDiagram>();

        public virtual DbSet<Airport> Airports => Set<Airport>();

        public virtual DbSet<CommunicationFrequency> CommunicationFrequencies => Set<CommunicationFrequency>();

        public virtual DbSet<Airspace> Airspaces => Set<Airspace>();

        public virtual DbSet<SpecialUseAirspace> SpecialUseAirspaces => Set<SpecialUseAirspace>();

        public virtual DbSet<FaaPublicationCycle> FaaPublicationCycles => Set<FaaPublicationCycle>();   
        
        public virtual DbSet<Flight> Flights => Set<Flight>();
        
        public virtual DbSet<AircraftPerformanceProfile> AircraftPerformanceProfiles => Set<AircraftPerformanceProfile>();

        public virtual DbSet<Runway> Runways => Set<Runway>();

        public virtual DbSet<RunwayEnd> RunwayEnds => Set<RunwayEnd>();

        public virtual DbSet<Obstacle> Obstacles => Set<Obstacle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(VFR3DDbContext).Assembly);
        }
    }
}
