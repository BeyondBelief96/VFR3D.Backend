using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRunwayAndRunwayEndEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "runways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_no = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    runway_id = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    length = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    surface_type_code = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true),
                    surface_treatment_code = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    pavement_classification = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true),
                    edge_light_intensity = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    weight_bearing_single_wheel = table.Column<int>(type: "integer", nullable: true),
                    weight_bearing_dual_wheel = table.Column<int>(type: "integer", nullable: true),
                    weight_bearing_dual_tandem = table.Column<int>(type: "integer", nullable: true),
                    weight_bearing_double_dual_tandem = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runways", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runways_airports_site_no",
                        column: x => x.site_no,
                        principalTable: "airports",
                        principalColumn: "site_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "runway_ends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    site_no = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    runway_id_ref = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    runway_fk = table.Column<Guid>(type: "uuid", nullable: true),
                    runway_end_id = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    true_alignment = table.Column<int>(type: "integer", nullable: true),
                    approach_type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    right_hand_traffic_pattern = table.Column<bool>(type: "boolean", nullable: false),
                    runway_markings_type = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    runway_markings_condition = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    elevation = table.Column<decimal>(type: "numeric(7,1)", nullable: true),
                    threshold_crossing_height = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    visual_glide_path_angle = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    displaced_threshold_lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    displaced_threshold_long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    displaced_threshold_elev = table.Column<decimal>(type: "numeric(7,1)", nullable: true),
                    displaced_threshold_length = table.Column<int>(type: "integer", nullable: true),
                    touchdown_zone_elev = table.Column<decimal>(type: "numeric(7,1)", nullable: true),
                    visual_glide_slope_indicator = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    runway_visual_range_equipment = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    runway_visibility_value_equipment = table.Column<bool>(type: "boolean", nullable: false),
                    approach_light_system = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true),
                    runway_end_lights = table.Column<bool>(type: "boolean", nullable: false),
                    centerline_lights = table.Column<bool>(type: "boolean", nullable: false),
                    touchdown_zone_lights = table.Column<bool>(type: "boolean", nullable: false),
                    controlling_object_description = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true),
                    controlling_object_marked_lighted = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true),
                    controlling_object_clearance_slope = table.Column<int>(type: "integer", nullable: true),
                    controlling_object_height_above_runway = table.Column<int>(type: "integer", nullable: true),
                    controlling_object_distance_from_runway = table.Column<int>(type: "integer", nullable: true),
                    controlling_object_centerline_offset = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runway_ends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runway_ends_runways_runway_fk",
                        column: x => x.runway_fk,
                        principalTable: "runways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_runway_ends_runway_fk",
                table: "runway_ends",
                column: "runway_fk");

            migrationBuilder.CreateIndex(
                name: "IX_runway_ends_runway_fk_runway_end_id",
                table: "runway_ends",
                columns: new[] { "runway_fk", "runway_end_id" },
                unique: true,
                filter: "\"runway_fk\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_runway_ends_site_no_runway_id_ref_runway_end_id",
                table: "runway_ends",
                columns: new[] { "site_no", "runway_id_ref", "runway_end_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runways_length",
                table: "runways",
                column: "length");

            migrationBuilder.CreateIndex(
                name: "IX_runways_site_no",
                table: "runways",
                column: "site_no");

            migrationBuilder.CreateIndex(
                name: "IX_runways_site_no_runway_id",
                table: "runways",
                columns: new[] { "site_no", "runway_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runways_surface_type_code",
                table: "runways",
                column: "surface_type_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "runway_ends");

            migrationBuilder.DropTable(
                name: "runways");
        }
    }
}
