using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseGlobalIdForAirspaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_special_use_airspaces",
                table: "special_use_airspaces");

            migrationBuilder.DropIndex(
                name: "IX_special_use_airspaces_global_id",
                table: "special_use_airspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_airspaces",
                table: "airspaces");

            migrationBuilder.DropIndex(
                name: "IX_airspaces_global_id",
                table: "airspaces");

            migrationBuilder.DropIndex(
                name: "IX_airspaces_object_id",
                table: "airspaces");

            migrationBuilder.DropColumn(
                name: "object_id",
                table: "special_use_airspaces");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "airspaces");

            migrationBuilder.DropColumn(
                name: "object_id",
                table: "airspaces");

            migrationBuilder.AlterColumn<string>(
                name: "global_id",
                table: "special_use_airspaces",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "global_id",
                table: "airspaces",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_special_use_airspaces",
                table: "special_use_airspaces",
                column: "global_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_airspaces",
                table: "airspaces",
                column: "global_id");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_global_id",
                table: "special_use_airspaces",
                column: "global_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_global_id",
                table: "airspaces",
                column: "global_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_special_use_airspaces",
                table: "special_use_airspaces");

            migrationBuilder.DropIndex(
                name: "IX_special_use_airspaces_global_id",
                table: "special_use_airspaces");

            migrationBuilder.DropPrimaryKey(
                name: "PK_airspaces",
                table: "airspaces");

            migrationBuilder.DropIndex(
                name: "IX_airspaces_global_id",
                table: "airspaces");

            migrationBuilder.AlterColumn<string>(
                name: "global_id",
                table: "special_use_airspaces",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "object_id",
                table: "special_use_airspaces",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "global_id",
                table: "airspaces",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "airspaces",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "object_id",
                table: "airspaces",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_special_use_airspaces",
                table: "special_use_airspaces",
                column: "object_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_airspaces",
                table: "airspaces",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_global_id",
                table: "special_use_airspaces",
                column: "global_id");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_global_id",
                table: "airspaces",
                column: "global_id");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_object_id",
                table: "airspaces",
                column: "object_id",
                unique: true);
        }
    }
}
