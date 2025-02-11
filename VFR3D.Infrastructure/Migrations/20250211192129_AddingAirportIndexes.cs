using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingAirportIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_airports_state_code_arpt_id",
                table: "airports",
                columns: new[] { "state_code", "arpt_id" });

            migrationBuilder.CreateIndex(
                name: "IX_airports_state_code_icao_id",
                table: "airports",
                columns: new[] { "state_code", "icao_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_airports_state_code_arpt_id",
                table: "airports");

            migrationBuilder.DropIndex(
                name: "IX_airports_state_code_icao_id",
                table: "airports");
        }
    }
}
