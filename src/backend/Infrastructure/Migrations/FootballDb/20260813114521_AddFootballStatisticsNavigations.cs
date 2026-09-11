using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddFootballStatisticsNavigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamSeasonStatistics_CompetitionId",
                schema: "football",
                table: "FootballTeamSeasonStatistics",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballPlayerSeasonStatistics_TeamId",
                schema: "football",
                table: "FootballPlayerSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchTeamStatistics_TeamId",
                schema: "football",
                table: "FootballMatchTeamStatistics",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FootballMatchTeamStatistics_FootballMatches_MatchId",
                schema: "football",
                table: "FootballMatchTeamStatistics",
                column: "MatchId",
                principalSchema: "football",
                principalTable: "FootballMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballMatchTeamStatistics_FootballTeams_TeamId",
                schema: "football",
                table: "FootballMatchTeamStatistics",
                column: "TeamId",
                principalSchema: "football",
                principalTable: "FootballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballPlayerSeasonStatistics_FootballCompetitions_Competi~",
                schema: "football",
                table: "FootballPlayerSeasonStatistics",
                column: "CompetitionId",
                principalSchema: "football",
                principalTable: "FootballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballPlayerSeasonStatistics_FootballPlayers_PlayerId",
                schema: "football",
                table: "FootballPlayerSeasonStatistics",
                column: "PlayerId",
                principalSchema: "football",
                principalTable: "FootballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballPlayerSeasonStatistics_FootballTeams_TeamId",
                schema: "football",
                table: "FootballPlayerSeasonStatistics",
                column: "TeamId",
                principalSchema: "football",
                principalTable: "FootballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballTeamSeasonStatistics_FootballCompetitions_Competiti~",
                schema: "football",
                table: "FootballTeamSeasonStatistics",
                column: "CompetitionId",
                principalSchema: "football",
                principalTable: "FootballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballTeamSeasonStatistics_FootballTeams_TeamId",
                schema: "football",
                table: "FootballTeamSeasonStatistics",
                column: "TeamId",
                principalSchema: "football",
                principalTable: "FootballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FootballMatchTeamStatistics_FootballMatches_MatchId",
                schema: "football",
                table: "FootballMatchTeamStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballMatchTeamStatistics_FootballTeams_TeamId",
                schema: "football",
                table: "FootballMatchTeamStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballPlayerSeasonStatistics_FootballCompetitions_Competi~",
                schema: "football",
                table: "FootballPlayerSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballPlayerSeasonStatistics_FootballPlayers_PlayerId",
                schema: "football",
                table: "FootballPlayerSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballPlayerSeasonStatistics_FootballTeams_TeamId",
                schema: "football",
                table: "FootballPlayerSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballTeamSeasonStatistics_FootballCompetitions_Competiti~",
                schema: "football",
                table: "FootballTeamSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballTeamSeasonStatistics_FootballTeams_TeamId",
                schema: "football",
                table: "FootballTeamSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FootballTeamSeasonStatistics_CompetitionId",
                schema: "football",
                table: "FootballTeamSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FootballPlayerSeasonStatistics_TeamId",
                schema: "football",
                table: "FootballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FootballMatchTeamStatistics_TeamId",
                schema: "football",
                table: "FootballMatchTeamStatistics");
        }
    }
}
