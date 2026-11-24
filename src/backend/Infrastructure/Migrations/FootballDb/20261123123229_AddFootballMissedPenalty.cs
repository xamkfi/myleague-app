using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddFootballMissedPenalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShooterPlayerId",
                schema: "football",
                table: "FootballMatchEvents",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShooterPlayerId",
                schema: "football",
                table: "FootballMatchEvents");
        }
    }
}
