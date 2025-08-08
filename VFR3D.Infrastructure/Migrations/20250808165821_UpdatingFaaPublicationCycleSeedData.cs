using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingFaaPublicationCycleSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 3,
                column: "publication_type",
                value: "NasrSubscription_Airport");

            migrationBuilder.UpdateData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "cycle_length_days", "known_valid_date", "publication_type" },
                values: new object[] { 28, new DateTime(2025, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), "NasrSubscription_Frequencies" });

            migrationBuilder.InsertData(
                table: "faa_publication_cycle",
                columns: new[] { "Id", "cycle_length_days", "known_valid_date", "last_successful_update", "publication_type" },
                values: new object[] { 5, 56, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Airspaces" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 3,
                column: "publication_type",
                value: "NasrSubscription");

            migrationBuilder.UpdateData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "cycle_length_days", "known_valid_date", "publication_type" },
                values: new object[] { 56, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), "Airspaces" });
        }
    }
}
