using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddFooterContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FooterContacts",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FooterContacts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FooterContacts_SortOrder",
                schema: "common",
                table: "FooterContacts",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FooterContacts",
                schema: "common");
        }
    }
}
