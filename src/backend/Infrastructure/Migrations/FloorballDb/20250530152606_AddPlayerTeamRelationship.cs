using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorballDb
{
    /// <inheritdoc />
    public partial class AddPlayerTeamRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_PlayerId",
                table: "FloorballTeamPlayer",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballTeamPlayer_FloorballPlayers_PlayerId",
                table: "FloorballTeamPlayer",
                column: "PlayerId",
                principalTable: "FloorballPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballTeamPlayer_FloorballPlayers_PlayerId",
                table: "FloorballTeamPlayer");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamPlayer_PlayerId",
                table: "FloorballTeamPlayer");
        }
    }
}
