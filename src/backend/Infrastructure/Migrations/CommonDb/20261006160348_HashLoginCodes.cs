using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class HashLoginCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LoginCode",
                schema: "common",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.Sql(
                "UPDATE common.\"Users\" SET \"LoginCode\" = NULL, \"LoginCodeExpiresAt\" = NULL, \"LoginCodeAttempts\" = 0 WHERE \"LoginCode\" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE common.\"Users\" SET \"LoginCode\" = NULL, \"LoginCodeExpiresAt\" = NULL, \"LoginCodeAttempts\" = 0 WHERE \"LoginCode\" IS NOT NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "LoginCode",
                schema: "common",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
