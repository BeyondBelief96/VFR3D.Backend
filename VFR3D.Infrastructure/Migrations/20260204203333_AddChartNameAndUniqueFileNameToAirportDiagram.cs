using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChartNameAndUniqueFileNameToAirportDiagram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, delete all existing data - it will be repopulated by the cron job
            // This is necessary because we're adding a unique constraint and the old data
            // has duplicates (multiple airports can have multiple diagrams with different filenames,
            // but the old logic created duplicate entries)
            migrationBuilder.Sql("DELETE FROM airport_diagrams;");

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                table: "airport_diagrams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chart_name",
                table: "airport_diagrams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_airport_diagrams_file_name",
                table: "airport_diagrams",
                column: "file_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_airport_diagrams_file_name",
                table: "airport_diagrams");

            migrationBuilder.DropColumn(
                name: "chart_name",
                table: "airport_diagrams");

            migrationBuilder.AlterColumn<string>(
                name: "file_name",
                table: "airport_diagrams",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
