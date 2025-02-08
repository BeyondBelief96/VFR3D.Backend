using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingAirportEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "air_taxi_ops",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "annual_ops_date",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_gliders",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_hel",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_jet_eng",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_mil_acft",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_multi_eng",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_single_eng",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "based_ultralight_acft",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "commercial_ops",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "commuter_ops",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "itnrnt_ops",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "local_ops",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "mil_acft_ops",
                table: "airports");

            migrationBuilder.AlterColumn<string>(
                name: "twr_type_code",
                table: "airports",
                type: "varchar(12)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(12)");

            migrationBuilder.AlterColumn<string>(
                name: "resp_artcc_id",
                table: "airports",
                type: "varchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(4)");

            migrationBuilder.AlterColumn<string>(
                name: "ownership_type_code",
                table: "airports",
                type: "varchar(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2)");

            migrationBuilder.AlterColumn<string>(
                name: "min_op_network",
                table: "airports",
                type: "varchar(1)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)");

            migrationBuilder.AlterColumn<decimal>(
                name: "long_decimal",
                table: "airports",
                type: "numeric(11,8)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(11,8)");

            migrationBuilder.AlterColumn<decimal>(
                name: "lat_decimal",
                table: "airports",
                type: "numeric(10,8)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,8)");

            migrationBuilder.AlterColumn<string>(
                name: "inspector_code",
                table: "airports",
                type: "varchar(1)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1)");

            migrationBuilder.AlterColumn<string>(
                name: "fss_name",
                table: "airports",
                type: "varchar(30)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)");

            migrationBuilder.AlterColumn<string>(
                name: "fss_id",
                table: "airports",
                type: "varchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(4)");

            migrationBuilder.AlterColumn<string>(
                name: "facility_use_code",
                table: "airports",
                type: "varchar(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "elev",
                table: "airports",
                type: "numeric(6,1)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,1)");

            migrationBuilder.AlterColumn<string>(
                name: "county_name",
                table: "airports",
                type: "varchar(21)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(21)");

            migrationBuilder.AlterColumn<string>(
                name: "county_assoc_state",
                table: "airports",
                type: "varchar(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2)");

            migrationBuilder.AlterColumn<string>(
                name: "country_code",
                table: "airports",
                type: "varchar(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2)");

            migrationBuilder.AlterColumn<string>(
                name: "city",
                table: "airports",
                type: "varchar(40)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(40)");

            migrationBuilder.AlterColumn<string>(
                name: "arpt_status",
                table: "airports",
                type: "varchar(2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2)");

            migrationBuilder.AlterColumn<string>(
                name: "arpt_name",
                table: "airports",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "arpt_id",
                table: "airports",
                type: "varchar(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(4)");

            migrationBuilder.AddColumn<int>(
                name: "lat_deg",
                table: "airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lat_hemis",
                table: "airports",
                type: "varchar(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "lat_min",
                table: "airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "lat_sec",
                table: "airports",
                type: "numeric(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "long_deg",
                table: "airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "long_hemis",
                table: "airports",
                type: "varchar(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "long_min",
                table: "airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "long_sec",
                table: "airports",
                type: "numeric(6,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lat_deg",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "lat_hemis",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "lat_min",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "lat_sec",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "long_deg",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "long_hemis",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "long_min",
                table: "airports");

            migrationBuilder.DropColumn(
                name: "long_sec",
                table: "airports");

            migrationBuilder.AlterColumn<string>(
                name: "twr_type_code",
                table: "airports",
                type: "varchar(12)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(12)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "resp_artcc_id",
                table: "airports",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ownership_type_code",
                table: "airports",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "min_op_network",
                table: "airports",
                type: "varchar(1)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "long_decimal",
                table: "airports",
                type: "numeric(11,8)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(11,8)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "lat_decimal",
                table: "airports",
                type: "numeric(10,8)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,8)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "inspector_code",
                table: "airports",
                type: "varchar(1)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "fss_name",
                table: "airports",
                type: "varchar(30)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "fss_id",
                table: "airports",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "facility_use_code",
                table: "airports",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "elev",
                table: "airports",
                type: "numeric(6,1)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(6,1)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "county_name",
                table: "airports",
                type: "varchar(21)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(21)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "county_assoc_state",
                table: "airports",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "country_code",
                table: "airports",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "city",
                table: "airports",
                type: "varchar(40)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "arpt_status",
                table: "airports",
                type: "varchar(2)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "arpt_name",
                table: "airports",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "arpt_id",
                table: "airports",
                type: "varchar(4)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(4)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "air_taxi_ops",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "annual_ops_date",
                table: "airports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_gliders",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_hel",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_jet_eng",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_mil_acft",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_multi_eng",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_single_eng",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "based_ultralight_acft",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "commercial_ops",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "commuter_ops",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "itnrnt_ops",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "local_ops",
                table: "airports",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mil_acft_ops",
                table: "airports",
                type: "integer",
                nullable: true);
        }
    }
}
