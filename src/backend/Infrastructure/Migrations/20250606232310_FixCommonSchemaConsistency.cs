using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCommonSchemaConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Persons",
                table: "Persons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clubs",
                table: "Clubs");

            migrationBuilder.RenameTable(
                name: "Persons",
                newName: "Person",
                newSchema: "common");

            migrationBuilder.RenameTable(
                name: "Clubs",
                newName: "Club",
                newSchema: "common");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Person",
                schema: "common",
                table: "Person",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Club",
                schema: "common",
                table: "Club",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Person",
                schema: "common",
                table: "Person");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Club",
                schema: "common",
                table: "Club");

            migrationBuilder.RenameTable(
                name: "Person",
                schema: "common",
                newName: "Persons");

            migrationBuilder.RenameTable(
                name: "Club",
                schema: "common",
                newName: "Clubs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Persons",
                table: "Persons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clubs",
                table: "Clubs",
                column: "Id");
        }
    }
}
