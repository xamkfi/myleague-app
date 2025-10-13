using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class EnableSeasonToPlayerAndGoalieStatistics2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballPlayers_PlayerId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "PlayerId",
                principalSchema: "floorball",
                principalTable: "FloorballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "TeamId",
                principalSchema: "floorball",
                principalTable: "FloorballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballPlayers_PlayerId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");
        }
    }
}
