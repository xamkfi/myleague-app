using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class InitialHockeySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hockey");

            migrationBuilder.CreateTable(
                name: "HockeyCompetitions",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompetitionType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CompetitionRules_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompetitionRules_RuleBookVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CompetitionRules_RuleBookSource = table.Column<string>(type: "text", nullable: false),
                    CompetitionRules_Match_RegularPeriodCount = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Match_RegularPeriodLengthMinutes = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Match_OvertimeLengthMinutes = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Match_StopClock = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Match_OvertimeEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Match_ShootoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Match_OffsideEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Match_DelayedOffsideEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Match_IcingRule = table.Column<string>(type: "text", nullable: false),
                    CompetitionRules_Match_PenaltyShotEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Match_GoaliePullAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Standing_RegulationWinPoints = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Standing_OvertimeWinPoints = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Standing_ShootoutWinPoints = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Standing_OvertimeLossPoints = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Standing_ShootoutLossPoints = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Standing_TiePoints = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Standing_TieBreakers = table.Column<string>(type: "text", nullable: false),
                    CompetitionRules_Roster_MaxDressedPlayers = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Roster_MaxDressedGoalies = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Roster_MinDressedPlayers = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Roster_RequiresGoalie = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Roster_MaxCaptains = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Roster_MaxAlternateCaptains = table.Column<int>(type: "integer", nullable: false),
                    CompetitionRules_Roster_CanGoalieBeCaptain = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Roster_AllowGuestPlayers = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Roster_LineManagementEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CompetitionRules_Video_Enabled = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_CoachChallengeAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_ReviewGoals = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_ReviewOffsideBeforeGoal = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_ReviewGoalieInterference = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_ReviewHighStickGoal = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_ReviewPuckOverLine = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_Challenge_Enabled = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_Challenge_MaxChallengesPerTeam = table.Column<int>(type: "integer", nullable: true),
                    CompetitionRules_Video_Challenge_LoseChallengeAfterFailed = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_Challenge_PenaltyForFailedChallenge = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_Challenge_FailedChallengePenaltyMinutes = table.Column<int>(type: "integer", nullable: true),
                    CompetitionRules_Video_Challenge_FailedChallengePenaltyOffence = table.Column<string>(type: "text", nullable: true),
                    CompetitionRules_Video_Challenge_FailedChallengePenaltySeverity = table.Column<string>(type: "text", nullable: true),
                    CompetitionRules_Video_Challenge_AllowChallengeInOvertime = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Video_Challenge_AllowChallengeInShootout = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Contact_BodyCheckingAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Contact_OpenIceHitsAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Contact_FightingAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Contact_AutomaticGameMisconductForFight = table.Column<bool>(type: "boolean", nullable: true),
                    CompetitionRules_Contact_StrictHeadContactRule = table.Column<bool>(type: "boolean", nullable: true),
                    PlayoffSchedule = table.Column<string>(type: "jsonb", nullable: true),
                    SeasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChampionCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContentHtml = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CurrentStage = table.Column<string>(type: "text", nullable: true),
                    TournamentRules_Format = table.Column<string>(type: "text", nullable: true),
                    TournamentRules_HasGroupStage = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_HasPlayoffs = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_HasBronzeGame = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_HasPlacementGames = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_TeamsAdvancingPerGroup = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStanding_RegulationWinPoints = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStanding_OvertimeWinPoints = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStanding_ShootoutWinPoints = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStanding_OvertimeLossPoints = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStanding_ShootoutLossPoints = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStanding_TiePoints = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_MatchOverride_RegularPeriodCount = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_MatchOverride_RegularPeriodLengthMinutes = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_MatchOverride_OvertimeLengthMinutes = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_MatchOverride_StopClock = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_MatchOverride_OvertimeEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_MatchOverride_ShootoutEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_MatchOverride_OffsideEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_MatchOverride_DelayedOffsideEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_MatchOverride_IcingRule = table.Column<string>(type: "text", nullable: true),
                    TournamentRules_MatchOverride_PenaltyShotEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_MatchOverride_GoaliePullAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    HockeyTournament_ChampionCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyCompetitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HockeyOfficials",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OfficialRole = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MatchesOfficiated = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyOfficials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HockeyPlayers",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryPosition = table.Column<string>(type: "text", nullable: false),
                    Shoots = table.Column<string>(type: "text", nullable: false),
                    Catches = table.Column<string>(type: "text", nullable: true),
                    CareerGamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    CareerGoals = table.Column<int>(type: "integer", nullable: false),
                    CareerAssists = table.Column<int>(type: "integer", nullable: false),
                    CareerPenaltyMinutes = table.Column<int>(type: "integer", nullable: false),
                    CareerFaceoffWins = table.Column<int>(type: "integer", nullable: false),
                    CareerFaceoffAttempts = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HockeyTeams",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamCategory = table.Column<string>(type: "text", nullable: false),
                    HomeArena = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrimaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HockeyCompetitionTeams",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Seed = table.Column<int>(type: "integer", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyCompetitionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyCompetitionTeams_HockeyCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatches",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwayCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatches_HockeyCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyTournamentGroups",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyTournamentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyTournamentGroups_HockeyCompetitions_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyLines",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    LineType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyLines_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyTeamPlayers",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Position = table.Column<string>(type: "text", nullable: false),
                    CaptainRole = table.Column<string>(type: "text", nullable: false),
                    RosterStatus = table.Column<string>(type: "text", nullable: false),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    RequestedJerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyTeamPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyTeamPlayers_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyTeamPlayers_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyTeamStaff",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyTeamStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyTeamStaff_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyCompetitionDivisions",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ChampionCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    RulesOverride_Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RulesOverride_RuleBookVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RulesOverride_RuleBookSource = table.Column<string>(type: "text", nullable: true),
                    RulesOverride_Match_RegularPeriodCount = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Match_RegularPeriodLengthMinutes = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Match_OvertimeLengthMinutes = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Match_StopClock = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Match_OvertimeEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Match_ShootoutEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Match_OffsideEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Match_DelayedOffsideEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Match_IcingRule = table.Column<string>(type: "text", nullable: true),
                    RulesOverride_Match_PenaltyShotEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Match_GoaliePullAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Standing_RegulationWinPoints = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Standing_OvertimeWinPoints = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Standing_ShootoutWinPoints = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Standing_OvertimeLossPoints = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Standing_ShootoutLossPoints = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Standing_TiePoints = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Standing_TieBreakers = table.Column<string>(type: "text", nullable: true),
                    RulesOverride_Roster_MaxDressedPlayers = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Roster_MaxDressedGoalies = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Roster_MinDressedPlayers = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Roster_RequiresGoalie = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Roster_MaxCaptains = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Roster_MaxAlternateCaptains = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Roster_CanGoalieBeCaptain = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Roster_AllowGuestPlayers = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Roster_LineManagementEnabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_Enabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_CoachChallengeAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_ReviewGoals = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_ReviewOffsideBeforeGoal = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_ReviewGoalieInterference = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_ReviewHighStickGoal = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_ReviewPuckOverLine = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_Challenge_Enabled = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_Challenge_MaxChallengesPerTeam = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Video_Challenge_LoseChallengeAfterFailed = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_Challenge_PenaltyForFailedChallenge = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_Challenge_FailedChallengePenaltyMinutes = table.Column<int>(type: "integer", nullable: true),
                    RulesOverride_Video_Challenge_FailedChallengePenaltyOffence = table.Column<string>(type: "text", nullable: true),
                    RulesOverride_Video_Challenge_FailedChallengePenaltySeverity = table.Column<string>(type: "text", nullable: true),
                    RulesOverride_Video_Challenge_AllowChallengeInOvertime = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Video_Challenge_AllowChallengeInShootout = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Contact_BodyCheckingAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Contact_OpenIceHitsAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Contact_FightingAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Contact_AutomaticGameMisconductForFight = table.Column<bool>(type: "boolean", nullable: true),
                    RulesOverride_Contact_StrictHeadContactRule = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyCompetitionDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyCompetitionDivisions_HockeyCompetitionTeams_ChampionC~",
                        column: x => x.ChampionCompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyCompetitionDivisions_HockeyCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyPlayoffSeries",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Round = table.Column<string>(type: "text", nullable: false),
                    SeriesOrder = table.Column<int>(type: "integer", nullable: false),
                    BestOf = table.Column<int>(type: "integer", nullable: false),
                    HomeCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwayCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    HomeTeamWins = table.Column<int>(type: "integer", nullable: false),
                    AwayTeamWins = table.Column<int>(type: "integer", nullable: false),
                    WinnerCompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyPlayoffSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyPlayoffSeries_HockeyCompetitionTeams_AwayCompetitionT~",
                        column: x => x.AwayCompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyPlayoffSeries_HockeyCompetitionTeams_HomeCompetitionT~",
                        column: x => x.HomeCompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyPlayoffSeries_HockeyCompetitionTeams_WinnerCompetitio~",
                        column: x => x.WinnerCompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyPlayoffSeries_HockeyCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyTournamentGroupTeams",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Seed = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyTournamentGroupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyTournamentGroupTeams_HockeyCompetitionTeams_Competiti~",
                        column: x => x.CompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyTournamentGroupTeams_HockeyTournamentGroups_Tournamen~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyLinePlayers",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    LineId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyLinePlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyLinePlayers_HockeyLines_LineId",
                        column: x => x.LineId,
                        principalSchema: "hockey",
                        principalTable: "HockeyLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HockeyLinePlayers_HockeyTeamPlayers_TeamPlayerId",
                        column: x => x.TeamPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeamPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyCompetitionDivisionTeams",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    CompetitionDivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Seed = table.Column<int>(type: "integer", nullable: true),
                    StandingRank = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyCompetitionDivisionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyCompetitionDivisionTeams_HockeyCompetitionDivisions_C~",
                        column: x => x.CompetitionDivisionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HockeyCompetitionDivisionTeams_HockeyCompetitionTeams_Compe~",
                        column: x => x.CompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivision_Audit",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivision_CreatedAt",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivision_UpdatedAt",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisions_ChampionCompetitionTeamId",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                column: "ChampionCompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisions_Competition_Division",
                schema: "hockey",
                table: "HockeyCompetitionDivisions",
                columns: new[] { "CompetitionId", "DivisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisionTeam_Audit",
                schema: "hockey",
                table: "HockeyCompetitionDivisionTeams",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisionTeam_CreatedAt",
                schema: "hockey",
                table: "HockeyCompetitionDivisionTeams",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisionTeam_UpdatedAt",
                schema: "hockey",
                table: "HockeyCompetitionDivisionTeams",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisionTeams_CompetitionTeamId",
                schema: "hockey",
                table: "HockeyCompetitionDivisionTeams",
                column: "CompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionDivisionTeams_Division_CompetitionTeam_Active",
                schema: "hockey",
                table: "HockeyCompetitionDivisionTeams",
                columns: new[] { "CompetitionDivisionId", "CompetitionTeamId" },
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionTeam_Audit",
                schema: "hockey",
                table: "HockeyCompetitionTeams",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionTeam_CreatedAt",
                schema: "hockey",
                table: "HockeyCompetitionTeams",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionTeam_UpdatedAt",
                schema: "hockey",
                table: "HockeyCompetitionTeams",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitionTeams_Competition_Team_Active",
                schema: "hockey",
                table: "HockeyCompetitionTeams",
                columns: new[] { "CompetitionId", "TeamId" },
                unique: true,
                filter: "\"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLinePlayer_Audit",
                schema: "hockey",
                table: "HockeyLinePlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLinePlayer_CreatedAt",
                schema: "hockey",
                table: "HockeyLinePlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLinePlayer_UpdatedAt",
                schema: "hockey",
                table: "HockeyLinePlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLinePlayers_Line_TeamPlayer",
                schema: "hockey",
                table: "HockeyLinePlayers",
                columns: new[] { "LineId", "TeamPlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLinePlayers_TeamPlayerId",
                schema: "hockey",
                table: "HockeyLinePlayers",
                column: "TeamPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLine_Audit",
                schema: "hockey",
                table: "HockeyLines",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLine_CreatedAt",
                schema: "hockey",
                table: "HockeyLines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLine_UpdatedAt",
                schema: "hockey",
                table: "HockeyLines",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyLines_TeamId",
                schema: "hockey",
                table: "HockeyLines",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatch_Audit",
                schema: "hockey",
                table: "HockeyMatches",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatch_CreatedAt",
                schema: "hockey",
                table: "HockeyMatches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatch_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatches",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatches_CompetitionId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOfficial_Audit",
                schema: "hockey",
                table: "HockeyOfficials",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOfficial_CreatedAt",
                schema: "hockey",
                table: "HockeyOfficials",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOfficial_UpdatedAt",
                schema: "hockey",
                table: "HockeyOfficials",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOfficials_PersonId",
                schema: "hockey",
                table: "HockeyOfficials",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayer_Audit",
                schema: "hockey",
                table: "HockeyPlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayer_CreatedAt",
                schema: "hockey",
                table: "HockeyPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayer_UpdatedAt",
                schema: "hockey",
                table: "HockeyPlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayers_PersonId",
                schema: "hockey",
                table: "HockeyPlayers",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_Audit",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_AwayCompetitionTeamId",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                column: "AwayCompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_Competition_Round_Order",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                columns: new[] { "CompetitionId", "Round", "SeriesOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_CreatedAt",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_HomeCompetitionTeamId",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                column: "HomeCompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_UpdatedAt",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayoffSeries_WinnerCompetitionTeamId",
                schema: "hockey",
                table: "HockeyPlayoffSeries",
                column: "WinnerCompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamPlayer_Audit",
                schema: "hockey",
                table: "HockeyTeamPlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamPlayer_CreatedAt",
                schema: "hockey",
                table: "HockeyTeamPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamPlayer_UpdatedAt",
                schema: "hockey",
                table: "HockeyTeamPlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamPlayers_PlayerId",
                schema: "hockey",
                table: "HockeyTeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamPlayers_Team_Competition_Jersey_Active",
                schema: "hockey",
                table: "HockeyTeamPlayers",
                columns: new[] { "TeamId", "CompetitionId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL AND \"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamPlayers_Team_Player_Competition_Active",
                schema: "hockey",
                table: "HockeyTeamPlayers",
                columns: new[] { "TeamId", "PlayerId", "CompetitionId" },
                unique: true,
                filter: "\"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamStaff_Audit",
                schema: "hockey",
                table: "HockeyTeamStaff",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamStaff_CreatedAt",
                schema: "hockey",
                table: "HockeyTeamStaff",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamStaff_Team_Person_Role_Competition_Active",
                schema: "hockey",
                table: "HockeyTeamStaff",
                columns: new[] { "TeamId", "PersonId", "Role", "CompetitionId" },
                unique: true,
                filter: "\"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamStaff_UpdatedAt",
                schema: "hockey",
                table: "HockeyTeamStaff",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroup_Audit",
                schema: "hockey",
                table: "HockeyTournamentGroups",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroup_CreatedAt",
                schema: "hockey",
                table: "HockeyTournamentGroups",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroup_UpdatedAt",
                schema: "hockey",
                table: "HockeyTournamentGroups",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroups_Tournament_SortOrder",
                schema: "hockey",
                table: "HockeyTournamentGroups",
                columns: new[] { "TournamentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroupTeam_Audit",
                schema: "hockey",
                table: "HockeyTournamentGroupTeams",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroupTeam_CreatedAt",
                schema: "hockey",
                table: "HockeyTournamentGroupTeams",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroupTeam_UpdatedAt",
                schema: "hockey",
                table: "HockeyTournamentGroupTeams",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroupTeams_CompetitionTeamId",
                schema: "hockey",
                table: "HockeyTournamentGroupTeams",
                column: "CompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTournamentGroupTeams_Group_CompetitionTeam_Active",
                schema: "hockey",
                table: "HockeyTournamentGroupTeams",
                columns: new[] { "TournamentGroupId", "CompetitionTeamId" },
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HockeyCompetitionDivisionTeams",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyLinePlayers",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatches",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyOfficials",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyPlayoffSeries",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyTeamStaff",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyTournamentGroupTeams",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyCompetitionDivisions",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyLines",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyTeamPlayers",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyTournamentGroups",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyCompetitionTeams",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyPlayers",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyTeams",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyCompetitions",
                schema: "hockey");
        }
    }
}
