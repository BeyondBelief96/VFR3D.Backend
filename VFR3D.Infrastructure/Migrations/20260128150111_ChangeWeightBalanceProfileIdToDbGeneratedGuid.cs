using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VFR3D.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeWeightBalanceProfileIdToDbGeneratedGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL requires explicit USING clause to cast text to uuid
            migrationBuilder.Sql(
                "ALTER TABLE weight_balance_profiles ALTER COLUMN id TYPE uuid USING id::uuid;");

            // Set default value for new records to auto-generate UUID
            migrationBuilder.Sql(
                "ALTER TABLE weight_balance_profiles ALTER COLUMN id SET DEFAULT gen_random_uuid();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the default value
            migrationBuilder.Sql(
                "ALTER TABLE weight_balance_profiles ALTER COLUMN id DROP DEFAULT;");

            // Convert back to text
            migrationBuilder.Sql(
                "ALTER TABLE weight_balance_profiles ALTER COLUMN id TYPE text USING id::text;");
        }
    }
}
