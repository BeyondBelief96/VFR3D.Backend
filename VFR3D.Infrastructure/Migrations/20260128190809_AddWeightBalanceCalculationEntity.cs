using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using VFR3D.Domain.ValueObjects.WeightBalance;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWeightBalanceCalculationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weight_balance_calculations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    flight_id = table.Column<string>(type: "text", nullable: true),
                    weight_balance_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envelope_id = table.Column<string>(type: "text", nullable: true),
                    fuel_burn_gallons = table.Column<double>(type: "double precision", nullable: true),
                    loaded_stations = table.Column<List<StationLoad>>(type: "jsonb", nullable: false),
                    takeoff_result = table.Column<WeightBalanceCgResult>(type: "jsonb", nullable: false),
                    landing_result = table.Column<WeightBalanceCgResult>(type: "jsonb", nullable: true),
                    station_breakdown = table.Column<List<StationBreakdown>>(type: "jsonb", nullable: false),
                    envelope_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    envelope_limits = table.Column<List<CgEnvelopePoint>>(type: "jsonb", nullable: false),
                    warnings = table.Column<List<string>>(type: "jsonb", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_standalone = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weight_balance_calculations", x => x.id);
                    table.ForeignKey(
                        name: "FK_weight_balance_calculations_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_weight_balance_calculations_weight_balance_profiles_weight_~",
                        column: x => x.weight_balance_profile_id,
                        principalTable: "weight_balance_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_calculations_flight_id",
                table: "weight_balance_calculations",
                column: "flight_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_calculations_user_id",
                table: "weight_balance_calculations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_calculations_user_id_is_standalone",
                table: "weight_balance_calculations",
                columns: new[] { "user_id", "is_standalone" });

            migrationBuilder.CreateIndex(
                name: "IX_weight_balance_calculations_weight_balance_profile_id",
                table: "weight_balance_calculations",
                column: "weight_balance_profile_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weight_balance_calculations");
        }
    }
}
