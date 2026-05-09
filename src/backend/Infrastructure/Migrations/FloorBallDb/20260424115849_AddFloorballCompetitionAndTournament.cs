using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddFloorballCompetitionAndTournament : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Drop foreign keys that reference renamed tables/columns ────────
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatches_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballSeasonDivisions_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballSeasonDivisions");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballSeasonDivisionTeams_FloorballSeasonDivisions_Seaso~",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballSeasonTeam_FloorballSeasons_SeasonsId",
                schema: "floorball",
                table: "FloorballSeasonTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballSeasonTeam_FloorballTeams_TeamsId",
                schema: "floorball",
                table: "FloorballSeasonTeam");

            // ── 2. Drop indexes that reference columns we are about to rename ────
            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballStatisticsCache_SeasonId",
                schema: "floorball",
                table: "FloorballStatisticsCache");

            migrationBuilder.DropIndex(
                name: "IX_FloorballStatisticsCache_SeasonId_ExpiresAt",
                schema: "floorball",
                table: "FloorballStatisticsCache");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Assists",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Goals",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Points",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_GAA",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_SavePercentage",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_Wins",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballSeasonTeam_TeamsId",
                schema: "floorball",
                table: "FloorballSeasonTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloorballSeasons",
                schema: "floorball",
                table: "FloorballSeasons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloorballSeasonTeam",
                schema: "floorball",
                table: "FloorballSeasonTeam");

            // ── 3. Rename tables (preserve all existing rows) ─────────────────────
            migrationBuilder.RenameTable(
                name: "FloorballSeasons",
                schema: "floorball",
                newName: "FloorballCompetitions",
                newSchema: "floorball");

            migrationBuilder.RenameTable(
                name: "FloorballSeasonTeam",
                schema: "floorball",
                newName: "FloorballCompetitionTeam",
                newSchema: "floorball");

            // ── 4. Rename columns (preserve data) ────────────────────────────────
            migrationBuilder.RenameColumn(
                name: "SeasonsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam",
                newName: "CompetitionsId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                newName: "CompetitionId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                newName: "CompetitionId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                newName: "CompetitionId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                newName: "CompetitionId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                newName: "CompetitionId");

            migrationBuilder.RenameColumn(
                name: "SeasonDivisionId",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                newName: "CompetitionDivisionId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballSeasonDivisions",
                newName: "CompetitionId");

            migrationBuilder.RenameColumn(
                name: "SeasonId",
                schema: "floorball",
                table: "FloorballMatches",
                newName: "CompetitionId");

            migrationBuilder.RenameIndex(
                name: "IX_FloorballMatches_SeasonId",
                schema: "floorball",
                table: "FloorballMatches",
                newName: "IX_FloorballMatches_CompetitionId");

            // ── 5. Update column comments to reflect new semantics ───────────────
            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                type: "uuid",
                nullable: false,
                comment: "ID of the competition these statistics are for",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID of the season these statistics are for");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                type: "uuid",
                nullable: true,
                comment: "Optional competition ID this cache is associated with",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Optional season ID this cache is associated with");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                type: "uuid",
                nullable: false,
                comment: "ID of the competition these statistics are for",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID of the season these statistics are for");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                type: "uuid",
                nullable: false,
                comment: "ID of the competition these statistics are for",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID of the season these statistics are for");

            // ── 6. Add new tournament-specific columns on the TPH table ──────────
            migrationBuilder.AddColumn<Guid>(
                name: "TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentStage",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetitionType",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "Season");

            migrationBuilder.AddColumn<string>(
                name: "ContentHtml",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "character varying(50000)",
                maxLength: 50000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TournamentRules_GroupStage_AllowOvertime",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TournamentRules_GroupStage_AllowShootout",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_GroupStage_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_GroupStage_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_GroupStage_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TournamentRules_HasPlayoffStage",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TournamentRules_HasThirdPlaceMatch",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TournamentRules_Playoff_AllowOvertime",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TournamentRules_Playoff_AllowShootout",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_Playoff_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_Playoff_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_Playoff_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentRules_TeamsAdvancingPerGroup",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentStatus",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // ── 7. Re-create primary keys for renamed tables ─────────────────────
            migrationBuilder.AddPrimaryKey(
                name: "PK_FloorballCompetitions",
                schema: "floorball",
                table: "FloorballCompetitions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FloorballCompetitionTeam",
                schema: "floorball",
                table: "FloorballCompetitionTeam",
                columns: new[] { "CompetitionsId", "TeamsId" });

            // ── 8. Create new tournament-related tables ───────────────────────────
            migrationBuilder.CreateTable(
                name: "FloorballTournamentGroups",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTournamentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTournamentGroups_FloorballCompetitions_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "floorball",
                        principalTable: "FloorballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTournamentGroupTeams",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "FK_FloorballTournamentGroupTeams_FloorballTournamentGroups_Tou~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── 9. Recreate indexes using the new column names ────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                columns: new[] { "TeamId", "CompetitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_SeasonId",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_SeasonId_ExpiresAt",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                columns: new[] { "CompetitionId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "CompetitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Assists",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "CompetitionId", "Assists" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Goals",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "CompetitionId", "Goals" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Points",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "CompetitionId", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "CompetitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_GAA",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "CompetitionId", "GoalsAgainstAverage" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_SavePercentage",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "CompetitionId", "SavePercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_Wins",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "CompetitionId", "Wins" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballCompetitionTeam_TeamsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroups_Tournament_Order",
                schema: "floorball",
                table: "FloorballTournamentGroups",
                columns: new[] { "TournamentId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroupTeams_Group_Team",
                schema: "floorball",
                table: "FloorballTournamentGroupTeams",
                columns: new[] { "TournamentGroupId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTournamentGroupTeams_TeamId",
                schema: "floorball",
                table: "FloorballTournamentGroupTeams",
                column: "TeamId");

            // ── 10. Recreate foreign keys pointing to FloorballCompetitions ──────
            migrationBuilder.AddForeignKey(
                name: "FK_FloorballCompetitionTeam_FloorballCompetitions_CompetitionsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam",
                column: "CompetitionsId",
                principalSchema: "floorball",
                principalTable: "FloorballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballCompetitionTeam_FloorballTeams_TeamsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam",
                column: "TeamsId",
                principalSchema: "floorball",
                principalTable: "FloorballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballCompetitions_Compe~",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "CompetitionId",
                principalSchema: "floorball",
                principalTable: "FloorballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatches_FloorballCompetitions_CompetitionId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "CompetitionId",
                principalSchema: "floorball",
                principalTable: "FloorballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatches_FloorballTournamentGroups_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "TournamentGroupId",
                principalSchema: "floorball",
                principalTable: "FloorballTournamentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballCompetitions_Compe~",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "CompetitionId",
                principalSchema: "floorball",
                principalTable: "FloorballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballSeasonDivisions_FloorballCompetitions_CompetitionId",
                schema: "floorball",
                table: "FloorballSeasonDivisions",
                column: "CompetitionId",
                principalSchema: "floorball",
                principalTable: "FloorballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballSeasonDivisionTeams_FloorballSeasonDivisions_Compe~",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                column: "CompetitionDivisionId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasonDivisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballCompetitions_Competi~",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "CompetitionId",
                principalSchema: "floorball",
                principalTable: "FloorballCompetitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── 1. Drop new foreign keys ─────────────────────────────────────────
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballCompetitionTeam_FloorballCompetitions_CompetitionsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballCompetitionTeam_FloorballTeams_TeamsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballCompetitions_Compe~",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatches_FloorballCompetitions_CompetitionId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballMatches_FloorballTournamentGroups_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballCompetitions_Compe~",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballSeasonDivisions_FloorballCompetitions_CompetitionId",
                schema: "floorball",
                table: "FloorballSeasonDivisions");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballSeasonDivisionTeams_FloorballSeasonDivisions_Compe~",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams");

            migrationBuilder.DropForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballCompetitions_Competi~",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            // ── 2. Drop tournament-only tables ───────────────────────────────────
            migrationBuilder.DropTable(
                name: "FloorballTournamentGroupTeams",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTournamentGroups",
                schema: "floorball");

            // ── 3. Drop indexes referencing CompetitionId ────────────────────────
            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballStatisticsCache_SeasonId",
                schema: "floorball",
                table: "FloorballStatisticsCache");

            migrationBuilder.DropIndex(
                name: "IX_FloorballStatisticsCache_SeasonId_ExpiresAt",
                schema: "floorball",
                table: "FloorballStatisticsCache");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Assists",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Goals",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Points",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballMatches_TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_GAA",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_SavePercentage",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_Wins",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics");

            migrationBuilder.DropIndex(
                name: "IX_FloorballCompetitionTeam_TeamsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloorballCompetitions",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FloorballCompetitionTeam",
                schema: "floorball",
                table: "FloorballCompetitionTeam");

            // ── 4. Drop tournament-specific columns on TPH tables ────────────────
            migrationBuilder.DropColumn(
                name: "TournamentGroupId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "TournamentStage",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "CompetitionType",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "ContentHtml",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_GroupStage_AllowOvertime",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_GroupStage_AllowShootout",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_GroupStage_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_GroupStage_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_GroupStage_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_HasPlayoffStage",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_HasThirdPlaceMatch",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_Playoff_AllowOvertime",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_Playoff_AllowShootout",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_Playoff_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_Playoff_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_Playoff_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentRules_TeamsAdvancingPerGroup",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TournamentStatus",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "Venue",
                schema: "floorball",
                table: "FloorballCompetitions");

            // ── 5. Rename columns back to SeasonId ───────────────────────────────
            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                type: "uuid",
                nullable: false,
                comment: "ID of the season these statistics are for",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID of the competition these statistics are for");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                type: "uuid",
                nullable: true,
                comment: "Optional season ID this cache is associated with",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Optional competition ID this cache is associated with");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                type: "uuid",
                nullable: false,
                comment: "ID of the season these statistics are for",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID of the competition these statistics are for");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                type: "uuid",
                nullable: false,
                comment: "ID of the season these statistics are for",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "ID of the competition these statistics are for");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "CompetitionDivisionId",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                newName: "SeasonDivisionId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballSeasonDivisions",
                newName: "SeasonId");

            migrationBuilder.RenameColumn(
                name: "CompetitionId",
                schema: "floorball",
                table: "FloorballMatches",
                newName: "SeasonId");

            migrationBuilder.RenameIndex(
                name: "IX_FloorballMatches_CompetitionId",
                schema: "floorball",
                table: "FloorballMatches",
                newName: "IX_FloorballMatches_SeasonId");

            // ── 6. Rename tables back ────────────────────────────────────────────
            migrationBuilder.RenameColumn(
                name: "CompetitionsId",
                schema: "floorball",
                table: "FloorballCompetitionTeam",
                newName: "SeasonsId");

            migrationBuilder.RenameTable(
                name: "FloorballCompetitionTeam",
                schema: "floorball",
                newName: "FloorballSeasonTeam",
                newSchema: "floorball");

            migrationBuilder.RenameTable(
                name: "FloorballCompetitions",
                schema: "floorball",
                newName: "FloorballSeasons",
                newSchema: "floorball");

            // ── 7. Recreate primary keys ─────────────────────────────────────────
            migrationBuilder.AddPrimaryKey(
                name: "PK_FloorballSeasons",
                schema: "floorball",
                table: "FloorballSeasons",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FloorballSeasonTeam",
                schema: "floorball",
                table: "FloorballSeasonTeam",
                columns: new[] { "SeasonsId", "TeamsId" });

            // ── 8. Recreate indexes ──────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                columns: new[] { "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_SeasonId",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_SeasonId_ExpiresAt",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                columns: new[] { "SeasonId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Assists",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "SeasonId", "Assists" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Goals",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "SeasonId", "Goals" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Points",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "SeasonId", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_GAA",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "SeasonId", "GoalsAgainstAverage" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_SavePercentage",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "SeasonId", "SavePercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_Wins",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "SeasonId", "Wins" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonTeam_TeamsId",
                schema: "floorball",
                table: "FloorballSeasonTeam",
                column: "TeamsId");

            // ── 9. Recreate foreign keys to FloorballSeasons ─────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_FloorballSeasonTeam_FloorballSeasons_SeasonsId",
                schema: "floorball",
                table: "FloorballSeasonTeam",
                column: "SeasonsId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballSeasonTeam_FloorballTeams_TeamsId",
                schema: "floorball",
                table: "FloorballSeasonTeam",
                column: "TeamsId",
                principalSchema: "floorball",
                principalTable: "FloorballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballGoalieSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballMatches_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballPlayerSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballSeasonDivisions_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballSeasonDivisions",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballSeasonDivisionTeams_FloorballSeasonDivisions_Seaso~",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                column: "SeasonDivisionId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasonDivisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballTeamSeasonStatistics_FloorballSeasons_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "SeasonId",
                principalSchema: "floorball",
                principalTable: "FloorballSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
