using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddSeasonContentBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FootballSeasonContentBlocks",
                schema: "football",
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
                    table.PrimaryKey("PK_FootballSeasonContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballSeasonContentBlocks_FootballCompetitions_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "football",
                        principalTable: "FootballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FootballSeasonContentBlocks_Season_SortOrder",
                schema: "football",
                table: "FootballSeasonContentBlocks",
                columns: new[] { "SeasonId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FootballSeasonContentBlocks",
                schema: "football");
        }
    }
}
