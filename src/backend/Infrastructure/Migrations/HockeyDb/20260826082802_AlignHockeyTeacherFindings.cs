using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AlignHockeyTeacherFindings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_PenaltyForFailedChallenge",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_PenaltyForFailedChallenge");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_MaxChallengesPerTeam",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_MaxChallengesPerTeam");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_LoseChallengeAfterFailed",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_LoseChallengeAfterFailed");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_FailedChallengePenaltySeverity",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_FailedPenSeverity");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_FailedChallengePenaltyOffence",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_FailedPenOffence");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_FailedChallengePenaltyMinutes",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_FailedPenMinutes");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_Enabled",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_Enabled");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_AllowChallengeInShootout",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_AllowInShootout");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_Video_Challenge_AllowChallengeInOvertime",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_VidChal_AllowInOvertime");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_PenaltyForFailedChallenge",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_PenaltyForFailedChallenge");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_MaxChallengesPerTeam",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_MaxChallengesPerTeam");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_LoseChallengeAfterFailed",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_LoseChallengeAfterFailed");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_FailedChallengePenaltySeverity",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_FailedPenSeverity");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_FailedChallengePenaltyOffence",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_FailedPenOffence");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_FailedChallengePenaltyMinutes",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_FailedPenMinutes");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_Enabled",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_Enabled");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_AllowChallengeInShootout",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_AllowInShootout");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_Video_Challenge_AllowChallengeInOvertime",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_VidChal_AllowInOvertime");

            migrationBuilder.AddColumn<Guid>(
                name: "NextMatchId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextMatchSlot",
                schema: "hockey",
                table: "HockeyMatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayoffMatchOrder",
                schema: "hockey",
                table: "HockeyMatches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayoffRound",
                schema: "hockey",
                table: "HockeyMatches",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE hockey."HockeyCompetitions"
                ALTER COLUMN "CompetitionType" TYPE character varying(21)
                USING CASE "CompetitionType"
                    WHEN 1 THEN 'Season'
                    WHEN 2 THEN 'Tournament'
                    ELSE 'Season'
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchOfficials_OfficialId",
                schema: "hockey",
                table: "HockeyMatchOfficials",
                column: "OfficialId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatches_NextMatchId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "NextMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchActivePlayers_TeamPlayerId",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                column: "TeamPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatchActivePlayers_HockeyTeamPlayers_TeamPlayerId",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                column: "TeamPlayerId",
                principalSchema: "hockey",
                principalTable: "HockeyTeamPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatches_HockeyMatches_NextMatchId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "NextMatchId",
                principalSchema: "hockey",
                principalTable: "HockeyMatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatchOfficials_HockeyOfficials_OfficialId",
                schema: "hockey",
                table: "HockeyMatchOfficials",
                column: "OfficialId",
                principalSchema: "hockey",
                principalTable: "HockeyOfficials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatchTeams_HockeyTeams_TeamId",
                schema: "hockey",
                table: "HockeyMatchTeams",
                column: "TeamId",
                principalSchema: "hockey",
                principalTable: "HockeyTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatchActivePlayers_HockeyTeamPlayers_TeamPlayerId",
                schema: "hockey",
                table: "HockeyMatchActivePlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatches_HockeyMatches_NextMatchId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatchOfficials_HockeyOfficials_OfficialId",
                schema: "hockey",
                table: "HockeyMatchOfficials");

            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatchTeams_HockeyTeams_TeamId",
                schema: "hockey",
                table: "HockeyMatchTeams");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatchOfficials_OfficialId",
                schema: "hockey",
                table: "HockeyMatchOfficials");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatches_NextMatchId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatchActivePlayers_TeamPlayerId",
                schema: "hockey",
                table: "HockeyMatchActivePlayers");

            migrationBuilder.DropColumn(
                name: "NextMatchId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "NextMatchSlot",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "PlayoffMatchOrder",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "PlayoffRound",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_PenaltyForFailedChallenge",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_PenaltyForFailedChallenge");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_MaxChallengesPerTeam",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_MaxChallengesPerTeam");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_LoseChallengeAfterFailed",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_LoseChallengeAfterFailed");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_FailedPenSeverity",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_FailedChallengePenaltySeverity");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_FailedPenOffence",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_FailedChallengePenaltyOffence");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_FailedPenMinutes",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_FailedChallengePenaltyMinutes");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_Enabled",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_Enabled");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_AllowInShootout",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_AllowChallengeInShootout");

            migrationBuilder.RenameColumn(
                name: "CompetitionRules_VidChal_AllowInOvertime",
                schema: "hockey",
                table: "HockeyCompetitions",
                newName: "CompetitionRules_Video_Challenge_AllowChallengeInOvertime");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_PenaltyForFailedChallenge",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_PenaltyForFailedChallenge");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_MaxChallengesPerTeam",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_MaxChallengesPerTeam");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_LoseChallengeAfterFailed",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_LoseChallengeAfterFailed");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_FailedPenSeverity",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_FailedChallengePenaltySeverity");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_FailedPenOffence",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_FailedChallengePenaltyOffence");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_FailedPenMinutes",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_FailedChallengePenaltyMinutes");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_Enabled",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_Enabled");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_AllowInShootout",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_AllowChallengeInShootout");

            migrationBuilder.RenameColumn(
                name: "RulesOverride_VidChal_AllowInOvertime",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                newName: "RulesOverride_Video_Challenge_AllowChallengeInOvertime");

            migrationBuilder.Sql("""
                ALTER TABLE hockey."HockeyCompetitions"
                ALTER COLUMN "CompetitionType" TYPE integer
                USING CASE "CompetitionType"
                    WHEN 'Season' THEN 1
                    WHEN 'Tournament' THEN 2
                    ELSE 1
                END;
                """);
        }
    }
}
