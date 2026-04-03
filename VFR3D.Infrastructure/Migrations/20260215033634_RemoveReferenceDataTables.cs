using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReferenceDataTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "airport_diagrams");

            migrationBuilder.DropTable(
                name: "airsigmet");

            migrationBuilder.DropTable(
                name: "chart_supplement");

            migrationBuilder.DropTable(
                name: "communication_frequencies");

            migrationBuilder.DropTable(
                name: "faa_publication_cycle");

            migrationBuilder.DropTable(
                name: "flight_airspaces");

            migrationBuilder.DropTable(
                name: "flight_obstacles");

            migrationBuilder.DropTable(
                name: "flight_special_use_airspaces");

            migrationBuilder.DropTable(
                name: "gairmet");

            migrationBuilder.DropTable(
                name: "metar");

            migrationBuilder.DropTable(
                name: "pirep");

            migrationBuilder.DropTable(
                name: "runway_ends");

            migrationBuilder.DropTable(
                name: "taf");

            migrationBuilder.DropTable(
                name: "airspaces");

            migrationBuilder.DropTable(
                name: "obstacles");

            migrationBuilder.DropTable(
                name: "special_use_airspaces");

            migrationBuilder.DropTable(
                name: "runways");

            migrationBuilder.DropTable(
                name: "airports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "airport_diagrams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    airport_ident = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    airport_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    chart_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    file_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icao_ident = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airport_diagrams", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "airports",
                columns: table => new
                {
                    site_no = table.Column<string>(type: "varchar(9)", nullable: false),
                    acreage = table.Column<int>(type: "integer", nullable: true),
                    activation_date = table.Column<string>(type: "varchar(7)", nullable: true),
                    ado_code = table.Column<string>(type: "varchar(3)", nullable: true),
                    airframe_repair_ser_code = table.Column<string>(type: "varchar(5)", nullable: true),
                    arpt_elev_source = table.Column<string>(type: "varchar(16)", nullable: true),
                    arpt_id = table.Column<string>(type: "varchar(4)", nullable: true),
                    arpt_name = table.Column<string>(type: "varchar(50)", nullable: true),
                    arpt_psn_source = table.Column<string>(type: "varchar(16)", nullable: true),
                    arpt_status = table.Column<string>(type: "varchar(2)", nullable: true),
                    attendance_day = table.Column<string>(type: "varchar(16)", nullable: true),
                    attendance_hours = table.Column<string>(type: "varchar(40)", nullable: true),
                    attendance_month = table.Column<string>(type: "varchar(50)", nullable: true),
                    bcn_lens_color = table.Column<string>(type: "varchar(3)", nullable: true),
                    bcn_lgt_sked = table.Column<string>(type: "varchar(7)", nullable: true),
                    bottled_oxy_type = table.Column<string>(type: "varchar(8)", nullable: true),
                    bulk_oxy_type = table.Column<string>(type: "varchar(8)", nullable: true),
                    chart_name = table.Column<string>(type: "varchar(30)", nullable: true),
                    city = table.Column<string>(type: "varchar(40)", nullable: true),
                    contact_address1 = table.Column<string>(type: "varchar(35)", nullable: true),
                    contact_address2 = table.Column<string>(type: "varchar(35)", nullable: true),
                    contact_city = table.Column<string>(type: "varchar(30)", nullable: true),
                    contact_name = table.Column<string>(type: "varchar(35)", nullable: true),
                    contact_phone_number = table.Column<string>(type: "varchar(16)", nullable: true),
                    contact_state = table.Column<string>(type: "varchar(2)", nullable: true),
                    contact_title = table.Column<string>(type: "varchar(10)", nullable: true),
                    contact_zip_code = table.Column<string>(type: "varchar(5)", nullable: true),
                    contact_zip_plus_four = table.Column<string>(type: "varchar(4)", nullable: true),
                    contr_fuel_avbl = table.Column<string>(type: "varchar(1)", nullable: true),
                    country_code = table.Column<string>(type: "varchar(2)", nullable: true),
                    county_assoc_state = table.Column<string>(type: "varchar(2)", nullable: true),
                    county_name = table.Column<string>(type: "varchar(21)", nullable: true),
                    cta = table.Column<string>(type: "varchar(4)", nullable: true),
                    customs_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    direction_code = table.Column<string>(type: "varchar(3)", nullable: true),
                    dist_city_to_airport = table.Column<decimal>(type: "numeric(2,0)", nullable: true),
                    eff_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    elev = table.Column<decimal>(type: "numeric(6,1)", nullable: true),
                    elev_method_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    elevation_src_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    facility_use_code = table.Column<string>(type: "varchar(2)", nullable: true),
                    fss_id = table.Column<string>(type: "varchar(4)", nullable: true),
                    fss_name = table.Column<string>(type: "varchar(30)", nullable: true),
                    fss_on_arpt_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    fuel_types = table.Column<string>(type: "varchar(40)", nullable: true),
                    icao_id = table.Column<string>(type: "varchar(7)", nullable: true),
                    inspect_method_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    inspector_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    joint_use_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    last_info_response = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_inspection = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    lat_deg = table.Column<int>(type: "int", nullable: true),
                    lat_hemis = table.Column<string>(type: "varchar(1)", nullable: true),
                    lat_min = table.Column<int>(type: "int", nullable: true),
                    lat_sec = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    lgt_sked = table.Column<string>(type: "varchar(7)", nullable: true),
                    lndg_fee_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    lndg_rights_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    long_deg = table.Column<int>(type: "int", nullable: true),
                    long_hemis = table.Column<string>(type: "varchar(1)", nullable: true),
                    long_min = table.Column<int>(type: "int", nullable: true),
                    long_sec = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    mag_hemis = table.Column<string>(type: "varchar(1)", nullable: true),
                    mag_varn = table.Column<decimal>(type: "numeric(2,0)", nullable: true),
                    mag_varn_year = table.Column<int>(type: "integer", nullable: true),
                    medical_use_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    mil_lndg_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    min_op_network = table.Column<string>(type: "varchar(1)", nullable: true),
                    nasp_code = table.Column<string>(type: "varchar(7)", nullable: true),
                    notam_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    notam_id = table.Column<string>(type: "varchar(4)", nullable: true),
                    other_services = table.Column<string>(type: "varchar(110)", nullable: true),
                    ownership_type_code = table.Column<string>(type: "varchar(2)", nullable: true),
                    position_src_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pwr_plant_repair_ser = table.Column<string>(type: "varchar(5)", nullable: true),
                    region_code = table.Column<string>(type: "varchar(3)", nullable: true),
                    resp_artcc_id = table.Column<string>(type: "varchar(4)", nullable: true),
                    seg_circle_mkr_flag = table.Column<string>(type: "varchar(3)", nullable: true),
                    site_type_code = table.Column<string>(type: "varchar(1)", nullable: false),
                    sked_seq_no = table.Column<int>(type: "integer", nullable: true),
                    state_code = table.Column<string>(type: "varchar(2)", nullable: true),
                    state_name = table.Column<string>(type: "varchar(30)", nullable: true),
                    survey_method_code = table.Column<string>(type: "varchar(1)", nullable: true),
                    tpa = table.Column<int>(type: "integer", nullable: true),
                    trns_strg_buoy_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    trns_strg_hgr_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    trns_strg_tie_flag = table.Column<string>(type: "varchar(1)", nullable: true),
                    twr_type_code = table.Column<string>(type: "varchar(12)", nullable: true),
                    user_fee_flag = table.Column<string>(type: "varchar(26)", nullable: true),
                    wind_indcr_flag = table.Column<string>(type: "varchar(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airports", x => x.site_no);
                });

            migrationBuilder.CreateTable(
                name: "airsigmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    airsigmet_type = table.Column<string>(type: "text", nullable: true),
                    altitude = table.Column<string>(type: "jsonb", nullable: true),
                    area = table.Column<string>(type: "jsonb", nullable: true),
                    hazard = table.Column<string>(type: "jsonb", nullable: true),
                    movement_dir_degrees = table.Column<int>(type: "integer", nullable: true),
                    movement_speed_kt = table.Column<int>(type: "integer", nullable: true),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    valid_time_from = table.Column<string>(type: "text", nullable: true),
                    valid_time_to = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airsigmet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "airspaces",
                columns: table => new
                {
                    global_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    adhp_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    ak_high = table.Column<short>(type: "smallint", nullable: true),
                    ak_low = table.Column<short>(type: "smallint", nullable: true),
                    city = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    @class = table.Column<string>(name: "class", type: "varchar(200)", maxLength: 200, nullable: true),
                    comm_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    cont_agent = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    country = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    dst = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    exclusion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry(Polygon, 4326)", nullable: true),
                    gmt_offset = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    icao_id = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    ident = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    level = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    local_type = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_val = table.Column<double>(type: "double precision", nullable: true),
                    mil_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    onshore = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    pacific = table.Column<short>(type: "smallint", nullable: true),
                    sector = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    shape_area = table.Column<double>(type: "double precision", nullable: true),
                    shape_length = table.Column<double>(type: "double precision", nullable: true),
                    state = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    type_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_val = table.Column<double>(type: "double precision", nullable: true),
                    us_area = table.Column<short>(type: "smallint", nullable: true),
                    us_high = table.Column<short>(type: "smallint", nullable: true),
                    us_low = table.Column<short>(type: "smallint", nullable: true),
                    wkhr_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    wkhr_rmk = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airspaces", x => x.global_id);
                });

            migrationBuilder.CreateTable(
                name: "chart_supplement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    airport_city = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    airport_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    airport_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    navigational_aid_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chart_supplement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "communication_frequencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    artcc_or_fss_id = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true),
                    cpdlc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    effective_date = table.Column<DateTime>(type: "date", nullable: false),
                    facility_code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    facility_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    facility_type = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    frequency = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    frequency_use = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    primary_approach_radio_call = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: true),
                    remark = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: true),
                    sectorization = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    serviced_city = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    serviced_country = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    serviced_facility = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    serviced_facility_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    serviced_site_type = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    serviced_state = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    tower_hours = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    tower_or_comm_call = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communication_frequencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "faa_publication_cycle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cycle_length_days = table.Column<int>(type: "integer", nullable: false),
                    known_valid_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_successful_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publication_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_faa_publication_cycle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gairmet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    altitudes = table.Column<string>(type: "jsonb", nullable: true),
                    area = table.Column<string>(type: "jsonb", nullable: true),
                    due_to = table.Column<string>(type: "text", nullable: true),
                    expire_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    forecast_hour = table.Column<int>(type: "integer", nullable: false),
                    geometry_type = table.Column<string>(type: "text", nullable: true),
                    hazard_severity = table.Column<string>(type: "text", nullable: true),
                    hazard_type = table.Column<string>(type: "text", nullable: true),
                    issue_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    product = table.Column<string>(type: "text", nullable: false),
                    receipt_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tag = table.Column<string>(type: "text", nullable: true),
                    valid_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gairmet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "metar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    altim_in_hg = table.Column<float>(type: "real", nullable: true),
                    dewpoint_c = table.Column<float>(type: "real", nullable: true),
                    elevation_m = table.Column<float>(type: "real", nullable: true),
                    flight_category = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    latitude = table.Column<float>(type: "real", nullable: true),
                    longitude = table.Column<float>(type: "real", nullable: true),
                    maxT24hr_c = table.Column<float>(type: "real", nullable: true),
                    maxT_c = table.Column<float>(type: "real", nullable: true),
                    metar_type = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    minT24hr_c = table.Column<float>(type: "real", nullable: true),
                    minT_c = table.Column<float>(type: "real", nullable: true),
                    observation_time = table.Column<string>(type: "text", nullable: true),
                    pcp24hr_in = table.Column<float>(type: "real", nullable: true),
                    pcp3hr_in = table.Column<float>(type: "real", nullable: true),
                    pcp6hr_in = table.Column<float>(type: "real", nullable: true),
                    precip_in = table.Column<float>(type: "real", nullable: true),
                    quality_control_flags = table.Column<string>(type: "jsonb", nullable: true),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    sea_level_pressure_mb = table.Column<float>(type: "real", nullable: true),
                    sky_condition = table.Column<string>(type: "jsonb", nullable: true),
                    snow_in = table.Column<float>(type: "real", nullable: true),
                    station_id = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    temp_c = table.Column<float>(type: "real", nullable: true),
                    three_hr_pressure_tendency_mb = table.Column<float>(type: "real", nullable: true),
                    vert_vis_ft = table.Column<int>(type: "integer", nullable: true),
                    visibility_statute_mi = table.Column<string>(type: "text", nullable: true),
                    wind_dir_degrees = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    wind_gust_kt = table.Column<int>(type: "integer", nullable: true),
                    wind_speed_kt = table.Column<int>(type: "integer", nullable: true),
                    wx_string = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "obstacles",
                columns: table => new
                {
                    oas_number = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    action = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    city_name = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true),
                    country_id = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    faa_study_number = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true),
                    height_agl = table.Column<int>(type: "integer", nullable: true),
                    height_amsl = table.Column<int>(type: "integer", nullable: true),
                    horizontal_accuracy = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    julian_date = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    lat_degrees = table.Column<int>(type: "integer", nullable: true),
                    lat_hemisphere = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    lat_minutes = table.Column<int>(type: "integer", nullable: true),
                    lat_seconds = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    lighting = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    location = table.Column<Point>(type: "geography(Point, 4326)", nullable: true),
                    long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    long_degrees = table.Column<int>(type: "integer", nullable: true),
                    long_hemisphere = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    long_minutes = table.Column<int>(type: "integer", nullable: true),
                    long_seconds = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    mark_indicator = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    oas_code = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    obstacle_number = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false),
                    obstacle_type = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    state_id = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    verification_status = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    vertical_accuracy = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obstacles", x => x.oas_number);
                });

            migrationBuilder.CreateTable(
                name: "pirep",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aircraft_ref = table.Column<string>(type: "text", nullable: true),
                    altitude_ft_msl = table.Column<int>(type: "integer", nullable: true),
                    icing_condition = table.Column<string>(type: "jsonb", nullable: true),
                    latitude = table.Column<float>(type: "real", nullable: true),
                    longitude = table.Column<float>(type: "real", nullable: true),
                    observation_time = table.Column<string>(type: "text", nullable: true),
                    quality_control_flags = table.Column<string>(type: "jsonb", nullable: true),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    receipt_time = table.Column<string>(type: "text", nullable: true),
                    report_type = table.Column<string>(type: "text", nullable: true),
                    sky_condition = table.Column<string>(type: "jsonb", nullable: true),
                    temp_c = table.Column<float>(type: "real", nullable: true),
                    turbulence_condition = table.Column<string>(type: "jsonb", nullable: true),
                    vert_gust_kt = table.Column<int>(type: "integer", nullable: true),
                    visibility_statute_mi = table.Column<int>(type: "integer", nullable: true),
                    wind_dir_degrees = table.Column<int>(type: "integer", nullable: true),
                    wind_speed_kt = table.Column<int>(type: "integer", nullable: true),
                    wx_string = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pirep", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "special_use_airspaces",
                columns: table => new
                {
                    global_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ak_high = table.Column<short>(type: "smallint", nullable: true),
                    ak_low = table.Column<short>(type: "smallint", nullable: true),
                    city = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    @class = table.Column<string>(name: "class", type: "varchar(200)", maxLength: 200, nullable: true),
                    comm_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    cont_agent = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    country = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    dst_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    exclusion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    geometry = table.Column<Geometry>(type: "geometry(Polygon, 4326)", nullable: true),
                    gmt_offset = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    level_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    lower_val = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    onshore = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    pacific = table.Column<short>(type: "smallint", nullable: true),
                    remarks = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    sector = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    shape_area = table.Column<float>(type: "real", nullable: true),
                    shape_length = table.Column<float>(type: "real", nullable: true),
                    state = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    times_of_use = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    type_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_code = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_desc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_uom = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    upper_val = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    us_area = table.Column<short>(type: "smallint", nullable: true),
                    us_high = table.Column<short>(type: "smallint", nullable: true),
                    us_low = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_special_use_airspaces", x => x.global_id);
                });

            migrationBuilder.CreateTable(
                name: "taf",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bulletin_time = table.Column<string>(type: "text", nullable: true),
                    elevation_m = table.Column<float>(type: "real", nullable: true),
                    forecast = table.Column<string>(type: "jsonb", nullable: true),
                    issue_time = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<float>(type: "real", nullable: true),
                    longitude = table.Column<float>(type: "real", nullable: true),
                    raw_text = table.Column<string>(type: "text", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    station_id = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    valid_time_from = table.Column<string>(type: "text", nullable: true),
                    valid_time_to = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_taf", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "runways",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    edge_light_intensity = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    length = table.Column<int>(type: "integer", nullable: true),
                    pavement_classification = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true),
                    runway_id = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    site_no = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    surface_treatment_code = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    surface_type_code = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true),
                    weight_bearing_double_dual_tandem = table.Column<int>(type: "integer", nullable: true),
                    weight_bearing_dual_tandem = table.Column<int>(type: "integer", nullable: true),
                    weight_bearing_dual_wheel = table.Column<int>(type: "integer", nullable: true),
                    weight_bearing_single_wheel = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runways", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runways_airports_site_no",
                        column: x => x.site_no,
                        principalTable: "airports",
                        principalColumn: "site_no",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_airspaces",
                columns: table => new
                {
                    flight_id = table.Column<string>(type: "text", nullable: false),
                    airspace_global_id = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_airspaces", x => new { x.flight_id, x.airspace_global_id });
                    table.ForeignKey(
                        name: "FK_flight_airspaces_airspaces_airspace_global_id",
                        column: x => x.airspace_global_id,
                        principalTable: "airspaces",
                        principalColumn: "global_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_airspaces_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_obstacles",
                columns: table => new
                {
                    flight_id = table.Column<string>(type: "text", nullable: false),
                    obstacle_oas_number = table.Column<string>(type: "varchar(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_obstacles", x => new { x.flight_id, x.obstacle_oas_number });
                    table.ForeignKey(
                        name: "FK_flight_obstacles_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_obstacles_obstacles_obstacle_oas_number",
                        column: x => x.obstacle_oas_number,
                        principalTable: "obstacles",
                        principalColumn: "oas_number",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_special_use_airspaces",
                columns: table => new
                {
                    flight_id = table.Column<string>(type: "text", nullable: false),
                    special_use_airspace_global_id = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_special_use_airspaces", x => new { x.flight_id, x.special_use_airspace_global_id });
                    table.ForeignKey(
                        name: "FK_flight_special_use_airspaces_flights_flight_id",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_special_use_airspaces_special_use_airspaces_special_~",
                        column: x => x.special_use_airspace_global_id,
                        principalTable: "special_use_airspaces",
                        principalColumn: "global_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "runway_ends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    runway_fk = table.Column<Guid>(type: "uuid", nullable: true),
                    approach_light_system = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true),
                    approach_type = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    centerline_lights = table.Column<bool>(type: "boolean", nullable: false),
                    controlling_object_centerline_offset = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    controlling_object_clearance_slope = table.Column<int>(type: "integer", nullable: true),
                    controlling_object_description = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: true),
                    controlling_object_distance_from_runway = table.Column<int>(type: "integer", nullable: true),
                    controlling_object_height_above_runway = table.Column<int>(type: "integer", nullable: true),
                    controlling_object_marked_lighted = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true),
                    displaced_threshold_elev = table.Column<decimal>(type: "numeric(7,1)", nullable: true),
                    displaced_threshold_lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    displaced_threshold_length = table.Column<int>(type: "integer", nullable: true),
                    displaced_threshold_long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    elevation = table.Column<decimal>(type: "numeric(7,1)", nullable: true),
                    lat_decimal = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    long_decimal = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    right_hand_traffic_pattern = table.Column<bool>(type: "boolean", nullable: false),
                    runway_end_id = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    runway_end_lights = table.Column<bool>(type: "boolean", nullable: false),
                    runway_id_ref = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    runway_markings_condition = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    runway_markings_type = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true),
                    runway_visibility_value_equipment = table.Column<bool>(type: "boolean", nullable: false),
                    runway_visual_range_equipment = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    site_no = table.Column<string>(type: "varchar(9)", maxLength: 9, nullable: false),
                    threshold_crossing_height = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    touchdown_zone_elev = table.Column<decimal>(type: "numeric(7,1)", nullable: true),
                    touchdown_zone_lights = table.Column<bool>(type: "boolean", nullable: false),
                    true_alignment = table.Column<int>(type: "integer", nullable: true),
                    visual_glide_path_angle = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    visual_glide_slope_indicator = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_runway_ends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_runway_ends_runways_runway_fk",
                        column: x => x.runway_fk,
                        principalTable: "runways",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "faa_publication_cycle",
                columns: new[] { "Id", "cycle_length_days", "known_valid_date", "last_successful_update", "publication_type" },
                values: new object[,]
                {
                    { 1, 56, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "ChartSupplement" },
                    { 2, 28, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "AirportDiagram" },
                    { 3, 28, new DateTime(2025, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "NasrSubscription_Airport" },
                    { 4, 28, new DateTime(2025, 1, 23, 0, 0, 0, 0, DateTimeKind.Utc), null, "NasrSubscription_Frequencies" },
                    { 5, 56, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Airspaces" },
                    { 6, 56, new DateTime(2024, 12, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "SpecialUseAirspaces" },
                    { 7, 56, new DateTime(2025, 10, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, "Obstacles" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_airport_diagrams_airport_ident",
                table: "airport_diagrams",
                column: "airport_ident");

            migrationBuilder.CreateIndex(
                name: "IX_airport_diagrams_file_name",
                table: "airport_diagrams",
                column: "file_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_airport_diagrams_icao_ident",
                table: "airport_diagrams",
                column: "icao_ident");

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

            migrationBuilder.CreateIndex(
                name: "IX_airports_state_code_arpt_id",
                table: "airports",
                columns: new[] { "state_code", "arpt_id" });

            migrationBuilder.CreateIndex(
                name: "IX_airports_state_code_icao_id",
                table: "airports",
                columns: new[] { "state_code", "icao_id" });

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

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_class",
                table: "airspaces",
                column: "class");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_global_id",
                table: "airspaces",
                column: "global_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_icao_id",
                table: "airspaces",
                column: "icao_id");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_name",
                table: "airspaces",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_airspaces_state",
                table: "airspaces",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "IX_chart_supplement_airport_code",
                table: "chart_supplement",
                column: "airport_code");

            migrationBuilder.CreateIndex(
                name: "IX_chart_supplement_navigational_aid_name",
                table: "chart_supplement",
                column: "navigational_aid_name");

            migrationBuilder.CreateIndex(
                name: "IX_communication_frequencies_facility_code",
                table: "communication_frequencies",
                column: "facility_code");

            migrationBuilder.CreateIndex(
                name: "IX_communication_frequencies_facility_code_serviced_facility_s~",
                table: "communication_frequencies",
                columns: new[] { "facility_code", "serviced_facility", "serviced_site_type", "serviced_state", "frequency", "frequency_use", "sectorization" },
                unique: true,
                filter: "\"facility_code\" IS NOT NULL AND \"serviced_facility\" IS NOT NULL AND \"serviced_site_type\" IS NOT NULL AND \"serviced_state\" IS NOT NULL AND \"frequency\" IS NOT NULL AND \"frequency_use\" IS NOT NULL AND \"sectorization\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_communication_frequencies_serviced_facility",
                table: "communication_frequencies",
                column: "serviced_facility");

            migrationBuilder.CreateIndex(
                name: "IX_communication_frequencies_serviced_state",
                table: "communication_frequencies",
                column: "serviced_state");

            migrationBuilder.CreateIndex(
                name: "IX_faa_publication_cycle_publication_type",
                table: "faa_publication_cycle",
                column: "publication_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flight_airspaces_airspace_global_id",
                table: "flight_airspaces",
                column: "airspace_global_id");

            migrationBuilder.CreateIndex(
                name: "IX_flight_obstacles_obstacle_oas_number",
                table: "flight_obstacles",
                column: "obstacle_oas_number");

            migrationBuilder.CreateIndex(
                name: "IX_flight_special_use_airspaces_special_use_airspace_global_id",
                table: "flight_special_use_airspaces",
                column: "special_use_airspace_global_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_metar_observation_time",
                table: "metar",
                column: "observation_time");

            migrationBuilder.CreateIndex(
                name: "IX_metar_station_id",
                table: "metar",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_metar_station_id_observation_time",
                table: "metar",
                columns: new[] { "station_id", "observation_time" });

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_height_agl",
                table: "obstacles",
                column: "height_agl");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_height_amsl",
                table: "obstacles",
                column: "height_amsl");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_location",
                table: "obstacles",
                column: "location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_oas_number",
                table: "obstacles",
                column: "oas_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_obstacle_type",
                table: "obstacles",
                column: "obstacle_type");

            migrationBuilder.CreateIndex(
                name: "IX_obstacles_state_id",
                table: "obstacles",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "IX_pirep_latitude_longitude",
                table: "pirep",
                columns: new[] { "latitude", "longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_pirep_observation_time",
                table: "pirep",
                column: "observation_time");

            migrationBuilder.CreateIndex(
                name: "IX_pirep_receipt_time",
                table: "pirep",
                column: "receipt_time");

            migrationBuilder.CreateIndex(
                name: "IX_runway_ends_runway_fk",
                table: "runway_ends",
                column: "runway_fk");

            migrationBuilder.CreateIndex(
                name: "IX_runway_ends_runway_fk_runway_end_id",
                table: "runway_ends",
                columns: new[] { "runway_fk", "runway_end_id" },
                unique: true,
                filter: "\"runway_fk\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_runway_ends_site_no_runway_id_ref_runway_end_id",
                table: "runway_ends",
                columns: new[] { "site_no", "runway_id_ref", "runway_end_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runways_length",
                table: "runways",
                column: "length");

            migrationBuilder.CreateIndex(
                name: "IX_runways_site_no",
                table: "runways",
                column: "site_no");

            migrationBuilder.CreateIndex(
                name: "IX_runways_site_no_runway_id",
                table: "runways",
                columns: new[] { "site_no", "runway_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_runways_surface_type_code",
                table: "runways",
                column: "surface_type_code");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_class",
                table: "special_use_airspaces",
                column: "class");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_global_id",
                table: "special_use_airspaces",
                column: "global_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_name",
                table: "special_use_airspaces",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_special_use_airspaces_state",
                table: "special_use_airspaces",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "IX_taf_station_id",
                table: "taf",
                column: "station_id");

            migrationBuilder.CreateIndex(
                name: "IX_taf_station_id_valid_time_from",
                table: "taf",
                columns: new[] { "station_id", "valid_time_from" });

            migrationBuilder.CreateIndex(
                name: "IX_taf_valid_time_from",
                table: "taf",
                column: "valid_time_from");
        }
    }
}
