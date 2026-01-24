using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightObstacles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "obstacle_oas_numbers",
                table: "flights",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "flight_obstacles",
                columns: table => new
                {
                    flight_id = table.Column<string>(type: "text", nullable: false),
                    obstacle_oas_number = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_obstacles", x => new { x.flight_id, x.obstacle_oas_number });
                    table.ForeignKey(
                        name: "FK_flight_obstacles_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_obstacles_obstacles_obstacle_oas_number",
                        column: x => x.obstacle_oas_number,
                        principalTable: "obstacles",
                        principalColumn: "oas_number",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_flight_obstacles_obstacle_oas_number",
                table: "flight_obstacles",
                column: "obstacle_oas_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "flight_obstacles");

            migrationBuilder.DropColumn(
                name: "obstacle_oas_numbers",
                table: "flights");
        }
    }
}
