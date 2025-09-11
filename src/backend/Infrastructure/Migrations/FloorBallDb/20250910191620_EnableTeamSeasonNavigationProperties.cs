using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class EnableTeamSeasonNavigationProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "TeamId",
                principalSchema: "floorball",
                principalTable: "FloorballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");
        }
    }
}
