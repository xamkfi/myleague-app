using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddLoanGoalkeeperFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLoanGoalkeeper",
                schema: "football",
                table: "FootballPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLoanGoalkeeper",
                schema: "football",
                table: "FootballPlayers");
        }
    }
}
