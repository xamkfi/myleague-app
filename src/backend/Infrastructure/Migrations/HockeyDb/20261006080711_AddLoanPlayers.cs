using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AddLoanPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLoanPlayer",
                schema: "hockey",
                table: "HockeyPlayers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoanPlayerNumber",
                schema: "hockey",
                table: "HockeyPlayers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLoanPlayer",
                schema: "hockey",
                table: "HockeyPlayers");

            migrationBuilder.DropColumn(
                name: "LoanPlayerNumber",
                schema: "hockey",
                table: "HockeyPlayers");
        }
    }
}
