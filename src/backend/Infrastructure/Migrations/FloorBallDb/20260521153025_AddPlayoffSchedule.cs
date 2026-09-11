using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddPlayoffSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlayoffSchedule",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayoffSchedule",
                schema: "floorball",
                table: "FloorballCompetitions");
        }
    }
}
