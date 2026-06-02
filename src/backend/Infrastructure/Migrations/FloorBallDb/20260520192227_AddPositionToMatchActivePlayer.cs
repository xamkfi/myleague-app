using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddPositionToMatchActivePlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Position",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Forward",
                comment: "Per-match field role: Forward, Center or Defender");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                schema: "floorball",
                table: "FloorballMatchActivePlayers");
        }
    }
}
