using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AddSeasonContentBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HockeySeasonContentBlocks",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContentHtml = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeySeasonContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeySeasonContentBlocks_HockeyCompetitions_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HockeySeasonContentBlocks_Season_SortOrder",
                schema: "hockey",
                table: "HockeySeasonContentBlocks",
                columns: new[] { "SeasonId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HockeySeasonContentBlocks",
                schema: "hockey");
        }
    }
}
