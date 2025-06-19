using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddDivisionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_IsActive",
                table: "Divisions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Level",
                table: "Divisions",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Name_SportType",
                table: "Divisions",
                columns: new[] { "Name", "SportType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_SportType",
                table: "Divisions",
                column: "SportType");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_SportType_IsActive",
                table: "Divisions",
                columns: new[] { "SportType", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Divisions");
        }
    }
}
