using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddCompetitionScopedTeamRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_TeamId_JerseyNumber",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_Team_Base_Jersey",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL AND \"CompetitionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_Team_Competition_Jersey",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "CompetitionId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL AND \"CompetitionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId_Base",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true,
                filter: "\"CompetitionId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId_CompetitionId",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId", "CompetitionId" },
                unique: true,
                filter: "\"CompetitionId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_Team_Base_Jersey",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_Team_Competition_Jersey",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId_Base",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId_CompetitionId",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.DropColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballTeamPlayers");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_JerseyNumber",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);
        }
    }
}
