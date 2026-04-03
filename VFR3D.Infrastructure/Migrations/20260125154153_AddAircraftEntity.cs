using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aircraft_id",
                table: "flights",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "aircraft_id",
                table: "aircraft_performance",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aircraft",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    tail_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    aircraft_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    call_sign = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    serial_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    primary_color = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    color2 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    color3 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    color4 = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    category = table.Column<string>(type: "text", nullable: false),
                    aircraft_home = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    airspeed_units = table.Column<string>(type: "text", nullable: false),
                    length_units = table.Column<string>(type: "text", nullable: false),
                    default_cruise_altitude = table.Column<int>(type: "integer", nullable: true),
                    max_ceiling = table.Column<int>(type: "integer", nullable: true),
                    glide_speed = table.Column<int>(type: "integer", nullable: true),
                    glide_ratio = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_flights_aircraft_id",
                table: "flights",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_performance_aircraft_id",
                table: "aircraft_performance",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_user_id",
                table: "aircraft",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_user_id_tail_number",
                table: "aircraft",
                columns: new[] { "user_id", "tail_number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_aircraft_performance_aircraft_aircraft_id",
                table: "aircraft_performance",
                column: "aircraft_id",
                principalTable: "aircraft",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_flights_aircraft_aircraft_id",
                table: "flights",
                column: "aircraft_id",
                principalTable: "aircraft",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aircraft_performance_aircraft_aircraft_id",
                table: "aircraft_performance");

            migrationBuilder.DropForeignKey(
                name: "FK_flights_aircraft_aircraft_id",
                table: "flights");

            migrationBuilder.DropTable(
                name: "aircraft");

            migrationBuilder.DropIndex(
                name: "IX_flights_aircraft_id",
                table: "flights");

            migrationBuilder.DropIndex(
                name: "IX_aircraft_performance_aircraft_id",
                table: "aircraft_performance");

            migrationBuilder.DropColumn(
                name: "aircraft_id",
                table: "flights");

            migrationBuilder.DropColumn(
                name: "aircraft_id",
                table: "aircraft_performance");
        }
    }
}
