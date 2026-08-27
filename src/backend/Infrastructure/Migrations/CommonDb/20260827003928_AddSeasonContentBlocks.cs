using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddSeasonContentBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SeasonContentBlocks",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sport = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonYear = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContentHtml = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonContentBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonContentBlocks_CompetitionId_SortOrder",
                schema: "common",
                table: "SeasonContentBlocks",
                columns: new[] { "CompetitionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonContentBlocks_Sport_SeasonYear_SortOrder",
                schema: "common",
                table: "SeasonContentBlocks",
                columns: new[] { "Sport", "SeasonYear", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeasonContentBlocks",
                schema: "common");
        }
    }
}
