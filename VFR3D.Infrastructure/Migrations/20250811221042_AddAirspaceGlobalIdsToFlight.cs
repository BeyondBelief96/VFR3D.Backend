using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAirspaceGlobalIdsToFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "airspace_global_ids",
                table: "flights",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "special_use_airspace_global_ids",
                table: "flights",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "flight_airspaces",
                columns: table => new
                {
                    flight_id = table.Column<string>(type: "text", nullable: false),
                    airspace_global_id = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_airspaces", x => new { x.flight_id, x.airspace_global_id });
                    table.ForeignKey(
                        name: "FK_flight_airspaces_airspaces_airspace_global_id",
                        column: x => x.airspace_global_id,
                        principalTable: "airspaces",
                        principalColumn: "global_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_airspaces_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_special_use_airspaces",
                columns: table => new
                {
                    flight_id = table.Column<string>(type: "text", nullable: false),
                    special_use_airspace_global_id = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_special_use_airspaces", x => new { x.flight_id, x.special_use_airspace_global_id });
                    table.ForeignKey(
                        name: "FK_flight_special_use_airspaces_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_special_use_airspaces_special_use_airspaces_special_~",
                        column: x => x.special_use_airspace_global_id,
                        principalTable: "special_use_airspaces",
                        principalColumn: "global_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_flight_airspaces_airspace_global_id",
                table: "flight_airspaces",
                column: "airspace_global_id");

            migrationBuilder.CreateIndex(
                name: "IX_flight_special_use_airspaces_special_use_airspace_global_id",
                table: "flight_special_use_airspaces",
                column: "special_use_airspace_global_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_airspaces");

            migrationBuilder.DropTable(
                name: "flight_special_use_airspaces");

            migrationBuilder.DropColumn(
                name: "airspace_global_ids",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "special_use_airspace_global_ids",
                table: "flights");
        }
    }
}
