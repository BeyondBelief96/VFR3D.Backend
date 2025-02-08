using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCommunicationFrequencyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "communication_frequencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    effective_date = table.Column<DateTime>(type: "date", nullable: false),
                    facility_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    facility_type = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    artcc_or_fss_id = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true),
                    cpdlc = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    tower_hours = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    serviced_facility = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    serviced_facility_name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    serviced_site_type = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric(10,8)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(11,8)", nullable: true),
                    serviced_city = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    serviced_state = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    serviced_country = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true),
                    tower_or_comm_call = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    primary_approach_radio_call = table.Column<string>(type: "varchar(26)", maxLength: 26, nullable: true),
                    frequency = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true),
                    sectorization = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    frequency_use = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: true),
                    remark = table.Column<string>(type: "varchar(1500)", maxLength: 1500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_communication_frequencies", x => x.Id);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "communication_frequencies");
        }
    }
}
