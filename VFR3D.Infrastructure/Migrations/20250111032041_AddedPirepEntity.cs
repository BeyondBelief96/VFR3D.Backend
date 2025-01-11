using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedPirepEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pirep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_time = table.Column<string>(type: "text", nullable: true),
                    observation_time = table.Column<string>(type: "text", nullable: true),
                    quality_control_flags = table.Column<string>(type: "jsonb", nullable: true),
                    aircraft_ref = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<float>(type: "real", nullable: true),
                    longitude = table.Column<float>(type: "real", nullable: true),
                    altitude_ft_msl = table.Column<int>(type: "integer", nullable: true),
                    sky_condition = table.Column<string>(type: "jsonb", nullable: true),
                    turbulence_condition = table.Column<string>(type: "jsonb", nullable: true),
                    icing_condition = table.Column<string>(type: "jsonb", nullable: true),
                    visibility_statute_mi = table.Column<int>(type: "integer", nullable: true),
                    wx_string = table.Column<string>(type: "text", nullable: true),
                    temp_c = table.Column<float>(type: "real", nullable: true),
                    wind_dir_degrees = table.Column<int>(type: "integer", nullable: true),
                    wind_speed_kt = table.Column<int>(type: "integer", nullable: true),
                    vert_gust_kt = table.Column<int>(type: "integer", nullable: true),
                    report_type = table.Column<string>(type: "text", nullable: true),
                    raw_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pirep", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pirep_latitude_longitude",
                table: "pirep",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_pirep_observation_time",
                table: "pirep",
                column: "observation_time");

            migrationBuilder.CreateIndex(
                name: "IX_pirep_receipt_time",
                table: "pirep",
                column: "receipt_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pirep");
        }
    }
}
