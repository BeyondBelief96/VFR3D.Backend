using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChartSupplementsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "chart_supplement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    airport_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    airport_city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    airport_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    navigational_aid_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chart_supplement", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chart_supplement_airport_code",
                table: "chart_supplement",
                column: "airport_code");

            migrationBuilder.CreateIndex(
                name: "IX_chart_supplement_navigational_aid_name",
                table: "chart_supplement",
                column: "navigational_aid_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chart_supplement");
        }
    }
}
