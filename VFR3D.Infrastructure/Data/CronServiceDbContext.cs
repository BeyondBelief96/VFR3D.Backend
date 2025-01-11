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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CronServiceDbContext).Assembly);
        }
    }
}
