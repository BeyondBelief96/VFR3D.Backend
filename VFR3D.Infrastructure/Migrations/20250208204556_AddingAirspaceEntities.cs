using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingAirspaceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "airspaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    object_id = table.Column<int>(type: "integer", nullable: false),
                    global_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ident = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    icao_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_val = table.Column<double>(type: "double precision", nullable: true),
                    upper_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_val = table.Column<double>(type: "double precision", nullable: true),
                    lower_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    type_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    local_type = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    @class = table.Column<string>(name: "class", type: "varchar(200)", maxLength: 200, nullable: true),
                    mil_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    comm_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    level = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    sector = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    onshore = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    exclusion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    wkhr_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    wkhr_rmk = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    dst = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    gmt_offset = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    cont_agent = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    city = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    state = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    country = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    adhp_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    us_high = table.Column<short>(type: "smallint", nullable: true),
                    ak_high = table.Column<short>(type: "smallint", nullable: true),
                    ak_low = table.Column<short>(type: "smallint", nullable: true),
                    us_low = table.Column<short>(type: "smallint", nullable: true),
                    us_area = table.Column<short>(type: "smallint", nullable: true),
                    pacific = table.Column<short>(type: "smallint", nullable: true),
                    shape_area = table.Column<double>(type: "double precision", nullable: true),
                    shape_length = table.Column<double>(type: "double precision", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry(Polygon, 4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "special_use_airspaces",
                columns: table => new
                {
                    object_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    global_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    type_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    @class = table.Column<string>(name: "class", type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_val = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_val = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    level_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    state = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    country = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    cont_agent = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    comm_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    sector = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    onshore = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    exclusion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    times_of_use = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    gmt_offset = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    dst_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    remarks = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ak_low = table.Column<short>(type: "smallint", nullable: true),
                    ak_high = table.Column<short>(type: "smallint", nullable: true),
                    us_low = table.Column<short>(type: "smallint", nullable: true),
                    us_high = table.Column<short>(type: "smallint", nullable: true),
                    us_area = table.Column<short>(type: "smallint", nullable: true),
                    pacific = table.Column<short>(type: "smallint", nullable: true),
                    shape_area = table.Column<float>(type: "real", nullable: true),
                    shape_length = table.Column<float>(type: "real", nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry(Polygon, 4326)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_special_use_airspaces", x => x.object_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_class",
                table: "airspaces",
                column: "class");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_global_id",
                table: "airspaces",
                column: "global_id");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_icao_id",
                table: "airspaces",
                column: "icao_id");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_name",
                table: "airspaces",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_object_id",
                table: "airspaces",
                column: "object_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_state",
                table: "airspaces",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_class",
                table: "special_use_airspaces",
                column: "class");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_global_id",
                table: "special_use_airspaces",
                column: "global_id");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_name",
                table: "special_use_airspaces",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_state",
                table: "special_use_airspaces",
                column: "state");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airspaces");

            migrationBuilder.DropTable(
                name: "special_use_airspaces");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
