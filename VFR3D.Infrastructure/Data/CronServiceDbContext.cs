using Microsoft.EntityFrameworkCore;
using Vfr3d.Domain.Entities;

namespace VFR3D.Infrastructure.Data
{
    public class CronServiceDbContext : DbContext
    {
        public CronServiceDbContext(DbContextOptions<CronServiceDbContext> options) : base(options)
        {
            
        }

        public DbSet<Metar> Metars => Set<Metar>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CronServiceDbContext).Assembly);
        }
    }
}
