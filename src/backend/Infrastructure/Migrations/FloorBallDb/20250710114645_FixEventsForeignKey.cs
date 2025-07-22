using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class FixEventsForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatchEvents_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvents_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballMatchId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatchEvents_FloorballMatches_FloorballMatchId1",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "FloorballMatchId1",
                principalSchema: "floorball",
                principalTable: "FloorballMatches",
                principalColumn: "Id");
        }
    }
}
