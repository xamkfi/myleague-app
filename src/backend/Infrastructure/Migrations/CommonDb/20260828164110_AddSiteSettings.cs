using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSettings",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessTokenExpirationMinutes = table.Column<int>(type: "integer", nullable: false),
                    RefreshTokenExpirationDays = table.Column<int>(type: "integer", nullable: false),
                    LoginCodeExpirationMinutes = table.Column<int>(type: "integer", nullable: false),
                    LoginCodeMaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    SessionExpiryWarningMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSettings",
                schema: "common");
        }
    }
}
