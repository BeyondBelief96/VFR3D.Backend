using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddObstacleEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "obstacles",
                columns: table => new
                {
                    oas_number = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    oas_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    obstacle_number = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false),
                    verification_status = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    country_id = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    state_id = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    city_name = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true),
                    lat_degrees = table.Column<int>(type: "integer", nullable: true),
                    lat_minutes = table.Column<int>(type: "integer", nullable: true),
                    lat_seconds = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    lat_hemisphere = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    long_degrees = table.Column<int>(type: "integer", nullable: true),
                    long_minutes = table.Column<int>(type: "integer", nullable: true),
                    long_seconds = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    long_hemisphere = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    obstacle_type = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    height_agl = table.Column<int>(type: "integer", nullable: true),
                    height_amsl = table.Column<int>(type: "integer", nullable: true),
                    lighting = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    horizontal_accuracy = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    vertical_accuracy = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    mark_indicator = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    faa_study_number = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true),
                    action = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    julian_date = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obstacles", x => x.oas_number);
                });

            migrationBuilder.InsertData(
                table: "faa_publication_cycle",
                columns: new[] { "Id", "cycle_length_days", "known_valid_date", "last_successful_update", "publication_type" },
                values: new object[] { 7, 56, new DateTime(2025, 10, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Obstacles" });

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_height_agl",
                table: "obstacles",
                column: "height_agl");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_height_amsl",
                table: "obstacles",
                column: "height_amsl");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_location",
                table: "obstacles",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_oas_number",
                table: "obstacles",
                column: "oas_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_obstacle_type",
                table: "obstacles",
                column: "obstacle_type");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_state_id",
                table: "obstacles",
                column: "state_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "obstacles");

            migrationBuilder.DeleteData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 7);
        }
    }
}
