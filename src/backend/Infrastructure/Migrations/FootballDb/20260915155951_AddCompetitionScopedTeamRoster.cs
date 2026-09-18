using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddCompetitionScopedTeamRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FootballTeamPlayer_TeamId_JerseyNumber",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionId",
                schema: "football",
                table: "FootballTeamPlayers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_Team_Base_Jersey",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL AND \"CompetitionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_Team_Competition_Jersey",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "CompetitionId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL AND \"CompetitionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId_Base",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true,
                filter: "\"CompetitionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId_CompetitionId",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId", "CompetitionId" },
                unique: true,
                filter: "\"CompetitionId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FootballTeamPlayer_Team_Base_Jersey",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FootballTeamPlayer_Team_Competition_Jersey",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId_Base",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId_CompetitionId",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                schema: "football",
                table: "FootballTeamPlayers");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_TeamId_JerseyNumber",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);
        }
    }
}
