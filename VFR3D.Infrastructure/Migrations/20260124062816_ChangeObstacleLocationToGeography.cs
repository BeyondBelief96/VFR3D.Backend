using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeObstacleLocationToGeography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing GiST index on geometry column
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_obstacles_location\";");

            // Convert the column from geometry to geography, preserving data
            migrationBuilder.Sql(
                "ALTER TABLE obstacles ALTER COLUMN location TYPE geography(Point, 4326) USING location::geography;");

            // Recreate the spatial index for geography
            migrationBuilder.Sql(
                "CREATE INDEX \"IX_obstacles_location\" ON obstacles USING gist (location);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the GiST index on geography column
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_obstacles_location\";");

            // Convert back from geography to geometry
            migrationBuilder.Sql(
                "ALTER TABLE obstacles ALTER COLUMN location TYPE geometry(Point, 4326) USING location::geometry;");

            // Recreate the spatial index for geometry
            migrationBuilder.Sql(
                "CREATE INDEX \"IX_obstacles_location\" ON obstacles USING gist (location);");
        }
    }
}
