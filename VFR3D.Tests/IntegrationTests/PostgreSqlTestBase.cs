using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using VFR3D.Infrastructure.Data;
using Xunit;

namespace VFR3D.Tests.IntegrationTests
{
    public class PostgreSqlTestBase : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer;
        protected VFR3DDbContext DbContext { get; private set; }
        protected string ConnectionString { get; private set; }

        protected PostgreSqlTestBase()
        {
            // Configure PostgreSQL container
            _dbContainer = new PostgreSqlBuilder()
                .WithImage("postgis/postgis:15-3.3")
                .WithDatabase("vfr3d_test")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCleanUp(true)
                .WithAutoRemove(true)
                .Build();
        }

        public virtual async Task InitializeAsync()
        {
            // Start container
            await _dbContainer.StartAsync();
            ConnectionString = _dbContainer.GetConnectionString();

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);
            dataSourceBuilder.EnableDynamicJson();

            // Create and configure DbContext
            var optionsBuilder = new DbContextOptionsBuilder<VFR3DDbContext>()
                    .UseNpgsql(dataSourceBuilder.Build(), options =>
                        options.UseNetTopologySuite());

            DbContext = new VFR3DDbContext(optionsBuilder.Options);

            // Create database schema
            await DbContext.Database.EnsureCreatedAsync();

            // Seed the database with test data
            await SeedDatabaseAsync();
        }

        protected virtual async Task SeedDatabaseAsync()
        {
            // Override in derived classes to seed database
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _dbContainer.StopAsync();
        }
    }
}
