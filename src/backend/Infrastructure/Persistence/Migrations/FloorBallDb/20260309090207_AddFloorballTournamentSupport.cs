using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Persistence.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddFloorballTournamentSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TournamentId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TournamentRound",
                schema: "floorball",
                table: "FloorballMatches",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FloorballTournaments",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DescriptionHtml = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MatchRules_NumberOfPeriods = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    MatchRules_PeriodDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    MatchRules_AllowOvertime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MatchRules_OvertimeDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    MatchRules_AllowShootout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PlayoffFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GroupStageAdvancingCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ImageUrls = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTournaments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTournamentGroups",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phase = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTournamentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTournamentGroups_FloorballTournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTournamentGroupTeams",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTournamentGroupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTournamentGroupTeams_FloorballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorballTournamentGroupTeams_FloorballTournamentGroups_Gro~",
                        column: x => x.GroupId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_TournamentId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "TournamentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FloorballMatches_SeasonOrTournament",
                schema: "floorball",
                table: "FloorballMatches",
                sql: "\"SeasonId\" IS NOT NULL OR \"TournamentId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroups_Tournament_Name_Phase",
                schema: "floorball",
                table: "FloorballTournamentGroups",
                columns: new[] { "TournamentId", "Name", "Phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroupTeams_Group_Team",
                schema: "floorball",
                table: "FloorballTournamentGroupTeams",
                columns: new[] { "GroupId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroupTeams_TeamId",
                schema: "floorball",
                table: "FloorballTournamentGroupTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroupTeams_Tournament_Team",
                schema: "floorball",
                table: "FloorballTournamentGroupTeams",
                columns: new[] { "TournamentId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournaments_StartDate",
                schema: "floorball",
                table: "FloorballTournaments",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournaments_Status",
                schema: "floorball",
                table: "FloorballTournaments",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatches_FloorballTournamentGroups_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "TournamentGroupId",
                principalSchema: "floorball",
                principalTable: "FloorballTournamentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatches_FloorballTournaments_TournamentId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "TournamentId",
                principalSchema: "floorball",
                principalTable: "FloorballTournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatches_FloorballTournamentGroups_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatches_FloorballTournaments_TournamentId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropTable(
                name: "FloorballTournamentGroupTeams",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTournamentGroups",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTournaments",
                schema: "floorball");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatches_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatches_TournamentId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FloorballMatches_SeasonOrTournament",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "TournamentRound",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.AlterColumn<Guid>(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
