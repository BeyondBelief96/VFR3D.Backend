using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    station_id = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    observation_time = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<float>(type: "real", nullable: true),
                    longitude = table.Column<float>(type: "real", nullable: true),
                    temp_c = table.Column<float>(type: "real", nullable: true),
                    dewpoint_c = table.Column<float>(type: "real", nullable: true),
                    wind_dir_degrees = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    wind_speed_kt = table.Column<int>(type: "integer", nullable: true),
                    wind_gust_kt = table.Column<int>(type: "integer", nullable: true),
                    visibility_statute_mi = table.Column<string>(type: "text", nullable: true),
                    altim_in_hg = table.Column<float>(type: "real", nullable: true),
                    sea_level_pressure_mb = table.Column<float>(type: "real", nullable: true),
                    quality_control_flags = table.Column<string>(type: "jsonb", nullable: true),
                    wx_string = table.Column<string>(type: "text", nullable: true),
                    sky_condition = table.Column<string>(type: "jsonb", nullable: true),
                    flight_category = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    three_hr_pressure_tendency_mb = table.Column<float>(type: "real", nullable: true),
                    maxT_c = table.Column<float>(type: "real", nullable: true),
                    minT_c = table.Column<float>(type: "real", nullable: true),
                    maxT24hr_c = table.Column<float>(type: "real", nullable: true),
                    minT24hr_c = table.Column<float>(type: "real", nullable: true),
                    precip_in = table.Column<float>(type: "real", nullable: true),
                    pcp3hr_in = table.Column<float>(type: "real", nullable: true),
                    pcp6hr_in = table.Column<float>(type: "real", nullable: true),
                    pcp24hr_in = table.Column<float>(type: "real", nullable: true),
                    snow_in = table.Column<float>(type: "real", nullable: true),
                    vert_vis_ft = table.Column<int>(type: "integer", nullable: true),
                    metar_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    elevation_m = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metar", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_metar_observation_time",
                table: "metar",
                column: "observation_time");

            migrationBuilder.CreateIndex(
                name: "IX_metar_station_id",
                table: "metar",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_metar_station_id_observation_time",
                table: "metar",
                columns: new[] { "station_id", "observation_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "metar");
        }
    }
}
