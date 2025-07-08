using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddedFloorballGoalConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballPlayers_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballPlayers_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_GoalType",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "IsOvertime",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "IsShootout",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "AssistingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_GoalType",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "GoalType");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "ScoringPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_GoalType",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.AddColumn<bool>(
                name: "IsOvertime",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShootout",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "AssistingPlayerId",
                filter: "\"AssistingPlayerId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_GoalType",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "GoalType",
                filter: "\"GoalType\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "ScoringPlayerId",
                filter: "\"ScoringPlayerId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballPlayers_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "AssistingPlayerId",
                principalSchema: "floorball",
                principalTable: "FloorballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballPlayers_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "ScoringPlayerId",
                principalSchema: "floorball",
                principalTable: "FloorballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
