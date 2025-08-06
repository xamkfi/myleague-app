using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddTimerStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimerStates",
                schema: "common",
                columns: table => new
                {
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PausedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalPausedDuration = table.Column<long>(type: "bigint", nullable: false),
                    IsRunning = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimerStates", x => x.MatchId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimerStates_IsRunning",
                schema: "common",
                table: "TimerStates",
                column: "IsRunning");

            migrationBuilder.CreateIndex(
                name: "IX_TimerStates_LastUpdated",
                schema: "common",
                table: "TimerStates",
                column: "LastUpdated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimerStates",
                schema: "common");
        }
    }
}
