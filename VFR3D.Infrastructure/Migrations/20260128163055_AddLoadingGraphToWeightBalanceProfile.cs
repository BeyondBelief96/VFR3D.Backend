using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLoadingGraphToWeightBalanceProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "loading_graph_format",
                table: "weight_balance_profiles",
                type: "text",
                nullable: false,
                defaultValue: "MomentDividedBy1000");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "loading_graph_format",
                table: "weight_balance_profiles");
        }
    }
}
