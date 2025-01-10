using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTafEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "taf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    station_id = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    issue_time = table.Column<string>(type: "text", nullable: true),
                    bulletin_time = table.Column<string>(type: "text", nullable: true),
                    valid_time_from = table.Column<string>(type: "text", nullable: true),
                    valid_time_to = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<float>(type: "real", nullable: true),
                    longitude = table.Column<float>(type: "real", nullable: true),
                    elevation_m = table.Column<float>(type: "real", nullable: true),
                    forecast = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_taf", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_taf_station_id",
                table: "taf",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_taf_station_id_valid_time_from",
                table: "taf",
                columns: new[] { "station_id", "valid_time_from" });

            migrationBuilder.CreateIndex(
                name: "IX_taf_valid_time_from",
                table: "taf",
                column: "valid_time_from");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "taf");
        }
    }
}
