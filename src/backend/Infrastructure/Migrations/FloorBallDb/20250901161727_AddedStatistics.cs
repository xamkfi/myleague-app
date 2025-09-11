using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddedStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloorballGoalieSeasonStatistics",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the goalie these statistics belong to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team the goalie played for"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the season these statistics are for"),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games played"),
                    GamesStarted = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games started"),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of wins"),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of losses"),
                    Ties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of ties"),
                    Saves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total saves made"),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots faced"),
                    SavePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Save percentage"),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Goals allowed"),
                    GoalsAgainstAverage = table.Column<decimal>(type: "numeric(4,2)", nullable: false, defaultValue: 0m, comment: "Goals against average"),
                    Shutouts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of shutouts"),
                    MinutesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total minutes played"),
                    PowerPlaySaves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play saves"),
                    PowerPlayShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play shots faced"),
                    PowerPlaySavePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Power play save percentage"),
                    ShortHandedSaves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed saves"),
                    ShortHandedShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed shots faced"),
                    ShortHandedSavePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Short-handed save percentage"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballGoalieSeasonStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatchTeamStatistics",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the match these statistics belong to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team these statistics are for"),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Shots on goal"),
                    ShotsTotal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots taken"),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Shot percentage"),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Faceoffs won"),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total faceoffs"),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Faceoff win percentage"),
                    PowerPlayOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play opportunities"),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play goals"),
                    PowerPlayMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play minutes"),
                    PenaltyKillOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty kill opportunities"),
                    PenaltyKillSuccess = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Successful penalty kills"),
                    ShortHandedGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed goals"),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty minutes"),
                    Hits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Hits delivered"),
                    BlockedShots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Shots blocked"),
                    Takeaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Takeaways"),
                    Giveaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Giveaways"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchTeamStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballPlayerSeasonStatistics",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the player these statistics belong to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team the player played for"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the season these statistics are for"),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games played"),
                    Goals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Goals scored"),
                    Assists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Assists made"),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total points (goals + assists)"),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty minutes"),
                    PlusMinusRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Plus/minus rating"),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Shots on goal"),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Shooting percentage"),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play goals"),
                    PowerPlayAssists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play assists"),
                    ShortHandedGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed goals"),
                    ShortHandedAssists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed assists"),
                    GameWinningGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Game-winning goals"),
                    OvertimeGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Overtime goals"),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Faceoffs won"),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total faceoffs taken"),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Faceoff win percentage"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballPlayerSeasonStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballStatisticsCache",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Unique cache key identifier"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Optional season ID this cache is associated with"),
                    JsonData = table.Column<string>(type: "text", nullable: false, comment: "Serialized JSON data"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When this cache entry was last updated"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When this cache entry expires"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballStatisticsCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamSeasonStatistics",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team these statistics belong to"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the season these statistics are for"),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games played"),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of wins"),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of losses"),
                    Ties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of ties/overtime losses"),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total points earned"),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total goals scored"),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total goals conceded"),
                    GoalDifference = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Goal difference (goals for - goals against)"),
                    ShotsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots taken"),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots faced"),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Shot percentage"),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play goals scored"),
                    PowerPlayOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play opportunities"),
                    PowerPlayPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Power play success percentage"),
                    ShortHandedGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed goals scored"),
                    PenaltyKillOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty kill opportunities"),
                    PenaltyKillPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Penalty kill success percentage"),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total penalty minutes"),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Faceoffs won"),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total faceoffs taken"),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Faceoff win percentage"),
                    HomeWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Home wins"),
                    HomeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Home losses"),
                    AwayWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Away wins"),
                    AwayLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Away losses"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeamSeasonStatistics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "PlayerId");

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
                name: "IX_FloorballGoalieSeasonStatistics_TeamId",
                schema: "floorball",
                table: "FloorballGoalieSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchTeamStatistics_MatchId",
                schema: "floorball",
                table: "FloorballMatchTeamStatistics",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchTeamStatistics_MatchId_TeamId",
                schema: "floorball",
                table: "FloorballMatchTeamStatistics",
                columns: new[] { "MatchId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchTeamStatistics_TeamId",
                schema: "floorball",
                table: "FloorballMatchTeamStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "PlayerId");

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
                name: "IX_FloorballPlayerSeasonStatistics_TeamId",
                schema: "floorball",
                table: "FloorballPlayerSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_CacheKey",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_ExpiresAt",
                schema: "floorball",
                table: "FloorballStatisticsCache",
                column: "ExpiresAt");

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
                name: "IX_FloorballTeamSeasonStatistics_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId_SeasonId",
                schema: "floorball",
                table: "FloorballTeamSeasonStatistics",
                columns: new[] { "TeamId", "SeasonId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballGoalieSeasonStatistics",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballMatchTeamStatistics",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballPlayerSeasonStatistics",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballStatisticsCache",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTeamSeasonStatistics",
                schema: "floorball");
        }
    }
}
