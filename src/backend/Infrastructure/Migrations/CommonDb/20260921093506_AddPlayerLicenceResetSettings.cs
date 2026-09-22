using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddPlayerLicenceResetSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastPlayerLicenceResetYear",
                schema: "common",
                table: "SiteSettings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerLicenceResetDay",
                schema: "common",
                table: "SiteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PlayerLicenceResetMonth",
                schema: "common",
                table: "SiteSettings",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.Sql(
                """
                UPDATE common."SiteSettings"
                SET "LastPlayerLicenceResetYear" = CASE
                    WHEN EXTRACT(MONTH FROM (CURRENT_TIMESTAMP AT TIME ZONE 'Europe/Helsinki')) > 5
                      OR (
                          EXTRACT(MONTH FROM (CURRENT_TIMESTAMP AT TIME ZONE 'Europe/Helsinki')) = 5
                          AND EXTRACT(DAY FROM (CURRENT_TIMESTAMP AT TIME ZONE 'Europe/Helsinki')) >= 1
                      )
                    THEN EXTRACT(YEAR FROM (CURRENT_TIMESTAMP AT TIME ZONE 'Europe/Helsinki'))
                    ELSE EXTRACT(YEAR FROM (CURRENT_TIMESTAMP AT TIME ZONE 'Europe/Helsinki')) - 1
                END
                WHERE "LastPlayerLicenceResetYear" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPlayerLicenceResetYear",
                schema: "common",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PlayerLicenceResetDay",
                schema: "common",
                table: "SiteSettings");

            migrationBuilder.DropColumn(
                name: "PlayerLicenceResetMonth",
                schema: "common",
                table: "SiteSettings");
        }
    }
}
