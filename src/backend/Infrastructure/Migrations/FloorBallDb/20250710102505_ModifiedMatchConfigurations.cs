using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class ModifiedMatchConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FloorballPenalty_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_SecondaryAssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "SecondaryAssistingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvents_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvents_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballMatchId1");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvents_FloorballPenalty_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballPenalty_FloorballMatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballMatchId",
                principalSchema: "floorball",
                principalTable: "FloorballMatches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballMatchId1",
                principalSchema: "floorball",
                principalTable: "FloorballMatches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballPenalty_Floo~",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballPenalty_FloorballMatchId",
                principalSchema: "floorball",
                principalTable: "FloorballMatches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballPenalty_Floo~",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_SecondaryAssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvents_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvents_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvents_FloorballPenalty_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "FloorballPenalty_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");
        }
    }
}
