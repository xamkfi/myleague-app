using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class FloorballSaves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FloorballSave_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GoalieId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasInOvertime",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasInShootout",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_GoalieId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "GoalieId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvents_FloorballSave_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballSave_FloorballMatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballSave_Floorba~",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballSave_FloorballMatchId",
                principalSchema: "floorball",
                principalTable: "FloorballMatches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballSave_Floorba~",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvent_GoalieId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvents_FloorballSave_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "FloorballSave_FloorballMatchId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "GoalieId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "WasInOvertime",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "WasInShootout",
                schema: "floorball",
                table: "FloorballMatchEvents");
        }
    }
}
