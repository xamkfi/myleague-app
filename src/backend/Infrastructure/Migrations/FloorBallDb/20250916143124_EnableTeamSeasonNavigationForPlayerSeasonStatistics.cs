using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class EnableTeamSeasonNavigationForPlayerSeasonStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballPlayers_PlayerId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "PlayerId",
                principalSchema: "floorball",
                principalTable: "FloorballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
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
                name: "FK_FloorballPlayerSeasonStatistics_FloorballPlayers_PlayerId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");
        }
    }
}
