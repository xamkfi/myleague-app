using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddedFloorballPenaltyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballPlayers_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_DurationInMinutes",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_PenaltyType",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_DurationInMinutes",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "DurationInMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PenaltyType",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PenaltyType");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_DurationInMinutes",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_PenaltyType",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_DurationInMinutes",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "DurationInMinutes",
                filter: "\"DurationInMinutes\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PenaltyType",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PenaltyType",
                filter: "\"PenaltyType\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PlayerId",
                filter: "\"PlayerId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballPlayers_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PlayerId",
                principalSchema: "floorball",
                principalTable: "FloorballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
