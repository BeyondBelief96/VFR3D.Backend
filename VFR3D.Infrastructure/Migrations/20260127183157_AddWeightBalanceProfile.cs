using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using VFR3D.Domain.ValueObjects.WeightBalance;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightBalanceProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weight_balance_profiles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    aircraft_id = table.Column<string>(type: "text", nullable: true),
                    profile_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    datum_description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    empty_weight = table.Column<double>(type: "double precision", nullable: false),
                    empty_weight_arm = table.Column<double>(type: "double precision", nullable: false),
                    max_ramp_weight = table.Column<double>(type: "double precision", nullable: true),
                    max_takeoff_weight = table.Column<double>(type: "double precision", nullable: false),
                    max_landing_weight = table.Column<double>(type: "double precision", nullable: true),
                    max_zero_fuel_weight = table.Column<double>(type: "double precision", nullable: true),
                    weight_units = table.Column<string>(type: "text", nullable: false),
                    arm_units = table.Column<string>(type: "text", nullable: false),
                    loading_stations = table.Column<List<LoadingStation>>(type: "jsonb", nullable: false),
                    cg_envelopes = table.Column<List<CgEnvelope>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weight_balance_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_weight_balance_profiles_aircraft_aircraft_id",
                        column: x => x.aircraft_id,
                        principalTable: "aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_profiles_aircraft_id",
                table: "weight_balance_profiles",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_profiles_user_id",
                table: "weight_balance_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_profiles_user_id_profile_name",
                table: "weight_balance_profiles",
                columns: new[] { "user_id", "profile_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weight_balance_profiles");
        }
    }
}
