using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAirportDiagramEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "airport_diagrams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    airport_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    icao_ident = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    airport_ident = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airport_diagrams", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_airport_diagrams_airport_ident",
                table: "airport_diagrams",
                column: "airport_ident");

            migrationBuilder.CreateIndex(
                name: "IX_airport_diagrams_icao_ident",
                table: "airport_diagrams",
                column: "icao_ident");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airport_diagrams");
        }
    }
}
