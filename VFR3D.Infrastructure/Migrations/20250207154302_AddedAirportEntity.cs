using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAirportEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "airports",
                columns: table => new
                {
                    site_no = table.Column<string>(type: "varchar(9)", nullable: false),
                    eff_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    site_type_code = table.Column<string>(type: "varchar(1)", nullable: false),
                    state_code = table.Column<string>(type: "varchar(2)", nullable: true),
                    arpt_id = table.Column<string>(type: "varchar(4)", nullable: false),
                    city = table.Column<string>(type: "varchar(40)", nullable: false),
                    country_code = table.Column<string>(type: "varchar(2)", nullable: false),
                    region_code = table.Column<string>(type: "varchar(3)", nullable: true),
                    ado_code = table.Column<string>(type: "varchar(3)", nullable: true),
                    state_name = table.Column<string>(type: "varchar(30)", nullable: true),
                    county_name = table.Column<string>(type: "varchar(21)", nullable: false),
                    county_assoc_state = table.Column<string>(type: "varchar(2)", nullable: false),
                    arpt_name = table.Column<string>(type: "varchar(50)", nullable: false),
                    ownership_type_code = table.Column<string>(type: "varchar(2)", nullable: false),
                    facility_use_code = table.Column<string>(type: "varchar(2)", nullable: false),
                    lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: false),
                    long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: false),
                    survey_method_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    elev = table.Column<decimal>(type: "numeric(6,1)", nullable: false),
                    elev_method_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    mag_varn = table.Column<decimal>(type: "numeric(2,0)", nullable: true),
                    mag_hemis = table.Column<string>(type: "varchar(1)", nullable: true),
                    mag_varn_year = table.Column<int>(type: "integer", nullable: true),
                    tpa = table.Column<int>(type: "integer", nullable: true),
                    chart_name = table.Column<string>(type: "varchar(30)", nullable: true),
                    dist_city_to_airport = table.Column<decimal>(type: "numeric(2,0)", nullable: true),
                    direction_code = table.Column<string>(type: "varchar(3)", nullable: true),
                    acreage = table.Column<int>(type: "integer", nullable: true),
                    resp_artcc_id = table.Column<string>(type: "varchar(4)", nullable: false),
                    fss_on_arpt_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    fss_id = table.Column<string>(type: "varchar(4)", nullable: false),
                    fss_name = table.Column<string>(type: "varchar(30)", nullable: false),
                    notam_id = table.Column<string>(type: "varchar(4)", nullable: true),
                    notam_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    activation_date = table.Column<string>(type: "varchar(7)", nullable: true),
                    arpt_status = table.Column<string>(type: "varchar(2)", nullable: false),
                    nasp_code = table.Column<string>(type: "varchar(7)", nullable: true),
                    customs_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    lndg_rights_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    joint_use_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    mil_lndg_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    inspect_method_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    inspector_code = table.Column<string>(type: "varchar(1)", nullable: false),
                    last_inspection = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_info_response = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fuel_types = table.Column<string>(type: "varchar(40)", nullable: true),
                    airframe_repair_ser_code = table.Column<string>(type: "varchar(5)", nullable: true),
                    pwr_plant_repair_ser = table.Column<string>(type: "varchar(5)", nullable: true),
                    bottled_oxy_type = table.Column<string>(type: "varchar(8)", nullable: true),
                    bulk_oxy_type = table.Column<string>(type: "varchar(8)", nullable: true),
                    lgt_sked = table.Column<string>(type: "varchar(7)", nullable: true),
                    bcn_lgt_sked = table.Column<string>(type: "varchar(7)", nullable: true),
                    twr_type_code = table.Column<string>(type: "varchar(12)", nullable: false),
                    seg_circle_mkr_flag = table.Column<string>(type: "varchar(3)", nullable: true),
                    bcn_lens_color = table.Column<string>(type: "varchar(3)", nullable: true),
                    lndg_fee_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    medical_use_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    based_single_eng = table.Column<int>(type: "integer", nullable: true),
                    based_multi_eng = table.Column<int>(type: "integer", nullable: true),
                    based_jet_eng = table.Column<int>(type: "integer", nullable: true),
                    based_hel = table.Column<int>(type: "integer", nullable: true),
                    based_gliders = table.Column<int>(type: "integer", nullable: true),
                    based_mil_acft = table.Column<int>(type: "integer", nullable: true),
                    based_ultralight_acft = table.Column<int>(type: "integer", nullable: true),
                    commercial_ops = table.Column<int>(type: "integer", nullable: true),
                    commuter_ops = table.Column<int>(type: "integer", nullable: true),
                    air_taxi_ops = table.Column<int>(type: "integer", nullable: true),
                    local_ops = table.Column<int>(type: "integer", nullable: true),
                    itnrnt_ops = table.Column<int>(type: "integer", nullable: true),
                    mil_acft_ops = table.Column<int>(type: "integer", nullable: true),
                    annual_ops_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arpt_psn_source = table.Column<string>(type: "varchar(16)", nullable: true),
                    position_src_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    arpt_elev_source = table.Column<string>(type: "varchar(16)", nullable: true),
                    elevation_src_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    contr_fuel_avbl = table.Column<string>(type: "varchar(1)", nullable: true),
                    trns_strg_buoy_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    trns_strg_hgr_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    trns_strg_tie_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    other_services = table.Column<string>(type: "varchar(110)", nullable: true),
                    wind_indcr_flag = table.Column<string>(type: "varchar(3)", nullable: true),
                    icao_id = table.Column<string>(type: "varchar(7)", nullable: true),
                    min_op_network = table.Column<string>(type: "varchar(1)", nullable: false),
                    user_fee_flag = table.Column<string>(type: "varchar(26)", nullable: true),
                    cta = table.Column<string>(type: "varchar(4)", nullable: true),
                    sked_seq_no = table.Column<int>(type: "integer", nullable: true),
                    attendance_month = table.Column<string>(type: "varchar(50)", nullable: true),
                    attendance_day = table.Column<string>(type: "varchar(16)", nullable: true),
                    attendance_hours = table.Column<string>(type: "varchar(40)", nullable: true),
                    contact_title = table.Column<string>(type: "varchar(10)", nullable: true),
                    contact_name = table.Column<string>(type: "varchar(35)", nullable: true),
                    contact_address1 = table.Column<string>(type: "varchar(35)", nullable: true),
                    contact_address2 = table.Column<string>(type: "varchar(35)", nullable: true),
                    contact_city = table.Column<string>(type: "varchar(30)", nullable: true),
                    contact_state = table.Column<string>(type: "varchar(2)", nullable: true),
                    contact_zip_code = table.Column<string>(type: "varchar(5)", nullable: true),
                    contact_zip_plus_four = table.Column<string>(type: "varchar(4)", nullable: true),
                    contact_phone_number = table.Column<string>(type: "varchar(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airports", x => x.site_no);
                });

            migrationBuilder.InsertData(
                table: "faa_publication_cycle",
                columns: new[] { "Id", "cycle_length_days", "known_valid_date", "last_successful_update", "publication_type" },
                values: new object[] { 3, 28, new DateTime(2025, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "NasrSubscription" });

            migrationBuilder.CreateIndex(
                name: "IX_airports_arpt_id",
                table: "airports",
                column: "arpt_id");

            migrationBuilder.CreateIndex(
                name: "IX_airports_icao_id",
                table: "airports",
                column: "icao_id");

            migrationBuilder.CreateIndex(
                name: "IX_airports_lat_decimal_long_decimal",
                table: "airports",
                columns: new[] { "lat_decimal", "long_decimal" });

            migrationBuilder.CreateIndex(
                name: "IX_airports_state_code",
                table: "airports",
                column: "state_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airports");

            migrationBuilder.DeleteData(
                table: "faa_publication_cycle",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
