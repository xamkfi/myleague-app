using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddFooterContactSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FooterContacts_SortOrder",
                schema: "common",
                table: "FooterContacts");

            migrationBuilder.AddColumn<string>(
                name: "Section",
                schema: "common",
                table: "FooterContacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Contact");

            migrationBuilder.CreateIndex(
                name: "IX_FooterContacts_Section_SortOrder",
                schema: "common",
                table: "FooterContacts",
                columns: new[] { "Section", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FooterContacts_Section_SortOrder",
                schema: "common",
                table: "FooterContacts");

            migrationBuilder.DropColumn(
                name: "Section",
                schema: "common",
                table: "FooterContacts");

            migrationBuilder.CreateIndex(
                name: "IX_FooterContacts_SortOrder",
                schema: "common",
                table: "FooterContacts",
                column: "SortOrder");
        }
    }
}
