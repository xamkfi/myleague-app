using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddPlayoffBracketSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NextMatchId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMatchSlot",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayoffMatchOrder",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayoffRound",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChampionTeamId",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_NextMatchId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "NextMatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatches_FloorballMatches_NextMatchId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "NextMatchId",
                principalSchema: "floorball",
                principalTable: "FloorballMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatches_FloorballMatches_NextMatchId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatches_NextMatchId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "NextMatchId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "NextMatchSlot",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "PlayoffMatchOrder",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "PlayoffRound",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "ChampionTeamId",
                schema: "floorball",
                table: "FloorballCompetitions");
        }
    }
}
