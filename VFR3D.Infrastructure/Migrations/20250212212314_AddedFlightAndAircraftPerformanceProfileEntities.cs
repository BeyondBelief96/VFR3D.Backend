using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using VFR3D.Domain.ValueObjects.Flights;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedFlightAndAircraftPerformanceProfileEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aircraft_performance",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    profile_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    climb_true_airspeed = table.Column<int>(type: "integer", nullable: false),
                    cruise_true_airspeed = table.Column<int>(type: "integer", nullable: false),
                    cruise_fuel_burn = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    climb_fuel_burn = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    descent_fuel_burn = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    climb_fpm = table.Column<int>(type: "integer", nullable: false),
                    descent_fpm = table.Column<int>(type: "integer", nullable: false),
                    descent_true_airspeed = table.Column<int>(type: "integer", nullable: false),
                    stt_fuel_gals = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    fuel_on_board_gals = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_performance", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flights",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    auth0_user_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    departure_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    planned_cruising_altitude = table.Column<int>(type: "integer", nullable: false),
                    waypoints = table.Column<List<Waypoint>>(type: "jsonb", nullable: false),
                    aircraft_performance_id = table.Column<string>(type: "text", nullable: false),
                    total_route_distance = table.Column<decimal>(type: "numeric(8,2)", nullable: false),
                    total_route_time_hours = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    total_fuel_used = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    average_wind_component = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    legs = table.Column<List<NavlogLeg>>(type: "jsonb", nullable: false),
                    state_codes_along_route = table.Column<List<string>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flights", x => x.id);
                    table.ForeignKey(
                        name: "FK_flights_aircraft_performance_aircraft_performance_id",
                        column: x => x.aircraft_performance_id,
                        principalTable: "aircraft_performance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_performance_user_id",
                table: "aircraft_performance",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_performance_user_id_profile_name",
                table: "aircraft_performance",
                columns: new[] { "user_id", "profile_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flights_aircraft_performance_id",
                table: "flights",
                column: "aircraft_performance_id");

            migrationBuilder.CreateIndex(
                name: "IX_flights_auth0_user_id",
                table: "flights",
                column: "auth0_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_flights_auth0_user_id_name",
                table: "flights",
                columns: new[] { "auth0_user_id", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flights");

            migrationBuilder.DropTable(
                name: "aircraft_performance");
        }
    }
}
