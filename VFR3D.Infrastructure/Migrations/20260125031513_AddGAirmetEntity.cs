using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGAirmetEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gairmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    receipt_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    issue_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expire_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    product = table.Column<string>(type: "text", nullable: false),
                    tag = table.Column<string>(type: "text", nullable: true),
                    forecast_hour = table.Column<int>(type: "integer", nullable: false),
                    hazard_type = table.Column<string>(type: "text", nullable: true),
                    hazard_severity = table.Column<string>(type: "text", nullable: true),
                    geometry_type = table.Column<string>(type: "text", nullable: true),
                    due_to = table.Column<string>(type: "text", nullable: true),
                    area = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gairmet", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gairmet_expire_time",
                table: "gairmet",
                column: "expire_time");

            migrationBuilder.CreateIndex(
                name: "IX_gairmet_hazard_type",
                table: "gairmet",
                column: "hazard_type");

            migrationBuilder.CreateIndex(
                name: "IX_gairmet_issue_time",
                table: "gairmet",
                column: "issue_time");

            migrationBuilder.CreateIndex(
                name: "IX_gairmet_product",
                table: "gairmet",
                column: "product");

            migrationBuilder.CreateIndex(
                name: "IX_gairmet_valid_time",
                table: "gairmet",
                column: "valid_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gairmet");
        }
    }
}
