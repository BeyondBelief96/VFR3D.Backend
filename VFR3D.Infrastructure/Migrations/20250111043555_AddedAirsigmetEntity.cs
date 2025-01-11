using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAirsigmetEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "airsigmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    valid_time_from = table.Column<string>(type: "text", nullable: true),
                    valid_time_to = table.Column<string>(type: "text", nullable: true),
                    altitude = table.Column<string>(type: "jsonb", nullable: true),
                    movement_dir_degrees = table.Column<int>(type: "integer", nullable: true),
                    movement_speed_kt = table.Column<int>(type: "integer", nullable: true),
                    hazard = table.Column<string>(type: "jsonb", nullable: true),
                    airsigmet_type = table.Column<string>(type: "text", nullable: true),
                    area = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airsigmet", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_airsigmet_airsigmet_type",
                table: "airsigmet",
                column: "airsigmet_type");

            migrationBuilder.CreateIndex(
                name: "IX_airsigmet_valid_time_from",
                table: "airsigmet",
                column: "valid_time_from");

            migrationBuilder.CreateIndex(
                name: "IX_airsigmet_valid_time_to",
                table: "airsigmet",
                column: "valid_time_to");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airsigmet");
        }
    }
}
