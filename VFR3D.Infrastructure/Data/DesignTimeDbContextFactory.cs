using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VFR3D.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VFR3DDbContext>
    {
        public VFR3DDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection") ??
                "Host=localhost;Database=vfr3d_development_database;Username=vfr3d_development_user;Password=localdevpassword;Port=5432";

            var optionsBuilder = new DbContextOptionsBuilder<VFR3DDbContext>();
            optionsBuilder.UseNpgsql(connectionString,
                npgsqlOptions => npgsqlOptions.UseNetTopologySuite());

            return new VFR3DDbContext(optionsBuilder.Options);
        }
    }
}