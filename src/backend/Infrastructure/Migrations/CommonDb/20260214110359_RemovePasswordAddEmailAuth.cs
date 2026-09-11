using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class RemovePasswordAddEmailAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Username",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "common",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "common",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "common",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                schema: "common",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoginCode",
                schema: "common",
                table: "Users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoginCodeAttempts",
                schema: "common",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LoginCodeExpiresAt",
                schema: "common",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByTokenId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "common",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                schema: "common",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_ActiveByUser",
                schema: "common",
                table: "RefreshTokens",
                columns: new[] { "UserId", "RevokedAt", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Audit",
                schema: "common",
                table: "RefreshTokens",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_CreatedAt",
                schema: "common",
                table: "RefreshTokens",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                schema: "common",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UpdatedAt",
                schema: "common",
                table: "RefreshTokens",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId",
                schema: "common",
                table: "RefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "common");

            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginCode",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginCodeAttempts",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LoginCodeExpiresAt",
                schema: "common",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "common",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "common",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                schema: "common",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
