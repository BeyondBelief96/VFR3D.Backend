using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingFaaPublicationCycleEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "faa_publication_cycle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    publication_type = table.Column<string>(type: "text", nullable: false),
                    cycle_length_days = table.Column<int>(type: "integer", nullable: false),
                    known_valid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_successful_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faa_publication_cycle", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "faa_publication_cycle",
                columns: new[] { "Id", "cycle_length_days", "known_valid_date", "last_successful_update", "publication_type" },
                values: new object[,]
                {
                    { 1, 56, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "ChartSupplement" },
                    { 2, 28, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "AirportDiagram" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_faa_publication_cycle_publication_type",
                table: "faa_publication_cycle",
                column: "publication_type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "faa_publication_cycle");
        }
    }
}
