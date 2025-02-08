using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingAirportEntityKeySiteNo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_airports",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "airports");

            migrationBuilder.AddPrimaryKey(
                name: "PK_airports",
                table: "airports",
                column: "site_no");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_airports",
                table: "airports");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "airports",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_airports",
                table: "airports",
                column: "Id");
        }
    }
}
