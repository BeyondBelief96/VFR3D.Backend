using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AircraftCascadeDeleteProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aircraft_performance_aircraft_aircraft_id",
                table: "aircraft_performance");

            migrationBuilder.AddForeignKey(
                name: "FK_aircraft_performance_aircraft_aircraft_id",
                table: "aircraft_performance",
                column: "aircraft_id",
                principalTable: "aircraft",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aircraft_performance_aircraft_aircraft_id",
                table: "aircraft_performance");

            migrationBuilder.AddForeignKey(
                name: "FK_aircraft_performance_aircraft_aircraft_id",
                table: "aircraft_performance",
                column: "aircraft_id",
                principalTable: "aircraft",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
