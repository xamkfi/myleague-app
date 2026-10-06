using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddLoanPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLoanPlayer",
                schema: "floorball",
                table: "FloorballPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoanPlayerNumber",
                schema: "floorball",
                table: "FloorballPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLoanPlayer",
                schema: "floorball",
                table: "FloorballPlayers");

            migrationBuilder.DropColumn(
                name: "LoanPlayerNumber",
                schema: "floorball",
                table: "FloorballPlayers");
        }
    }
}
