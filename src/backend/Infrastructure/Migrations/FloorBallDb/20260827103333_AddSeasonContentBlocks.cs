using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddSeasonContentBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloorballSeasonContentBlocks",
                schema: "floorball",
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
                    table.PrimaryKey("PK_FloorballSeasonContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonContentBlocks_FloorballCompetitions_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "floorball",
                        principalTable: "FloorballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonContentBlocks_Season_SortOrder",
                schema: "floorball",
                table: "FloorballSeasonContentBlocks",
                columns: new[] { "SeasonId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballSeasonContentBlocks",
                schema: "floorball");
        }
    }
}
