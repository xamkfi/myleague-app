using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Domain.Enums.Hockey.Teams;
using Domain.Enums.Football;

namespace Seeder;
public class PersonSeed
{
	public string FirstName { get; set; } = "John";
	public string LastName { get; set; } = "Doe";
	public string BirthDate { get; set; } = "1990-01-01";
	public bool IsRegistered { get; set; } = true;
	public AddressSeed? Address { get; set; }
	public ContactInfoSeed? ContactInfo { get; set; }
}

public class AddressSeed
{
	public string? Street1 { get; set; }
	public string? Street2 { get; set; }
	public string? City { get; set; }
	public string? PostalCode { get; set; }
	public string? Country { get; set; }
}

public class ContactInfoSeed
{
	public string? Email { get; set; }
	public string? Phone { get; set; }
	public string? AlternativePhone { get; set; }
}

public class ClubSeed
{
	public string Name { get; init; } = "Sample Club";
	public string City { get; init; } = "City";
	public string Country { get; init; } = "Country";
	public DateTime FoundingDate { get; init; } = new DateTime(2000, 1, 1);
	public string? WebsiteUrl { get; init; }
	public string? LogoUrl { get; init; }
	public string? ContactEmail { get; init; }
}

public class DivisionSeed
{
	public string Name { get; init; } = "First Division";
	public string Description { get; init; } = "Top level";
	public int Level { get; init; } = 1;
	public string SportType { get; init; } = "Floorball";
}

public class FloorballSeasonSeed
{
	public string Name { get; init; } = "2025 Regular Season";
	public string StartDate { get; init; } = "2025-01-01";
	public string EndDate { get; init; } = "2025-12-31";
	public List<string> DivisionNames { get; init; } = new List<string>();

	// Match rules configuration
	public int NumberOfPeriods { get; init; } = 2;
	public int PeriodDurationMinutes { get; init; } = 15;
	public bool AllowOvertime { get; init; } = true;
	public int OvertimeDurationMinutes { get; init; } = 5;
	public bool AllowShootout { get; init; } = true;
}

public class FloorballTournamentSeed
{
	public string Name { get; init; } = "2027 Spring Cup";
	public string StartDate { get; init; } = "2027-06-01";
	public string EndDate { get; init; } = "2027-06-30";
	public string? Venue { get; init; }
	public string? ContentHtml { get; init; }

	// Group-stage match rules
	public int GroupStageNumberOfPeriods { get; init; } = 3;
	public int GroupStagePeriodDurationMinutes { get; init; } = 20;
	public bool GroupStageAllowOvertime { get; init; } = false;
	public int GroupStageOvertimeDurationMinutes { get; init; } = 5;
	public bool GroupStageAllowShootout { get; init; } = false;

	// Playoff match rules
	public int PlayoffNumberOfPeriods { get; init; } = 3;
	public int PlayoffPeriodDurationMinutes { get; init; } = 20;
	public bool PlayoffAllowOvertime { get; init; } = true;
	public int PlayoffOvertimeDurationMinutes { get; init; } = 10;
	public bool PlayoffAllowShootout { get; init; } = true;

	// Structural flags
	public int TeamsAdvancingPerGroup { get; init; } = 2;
	public bool HasPlayoffStage { get; init; } = true;
	public bool HasThirdPlaceMatch { get; init; } = false;

	/// <summary>
	/// When true, the seeder transitions the tournament to GroupStage (if Draft), schedules every
	/// group-stage match in the past, and simulates each one through to completion. The result is a
	/// tournament that is ready for the admin to advance to the playoff stage — useful for testing
	/// playoff bracket generation without manually playing every match.
	/// </summary>
	public bool AllGroupMatchesCompleted { get; init; } = false;

	public List<FloorballTournamentGroupSeed> Groups { get; init; } = new List<FloorballTournamentGroupSeed>();
}

public class FloorballTournamentGroupSeed
{
	public string Name { get; init; } = "Group A";
	public List<string> TeamNames { get; init; } = new List<string>();
}

public class FloorballTeamSeed
{
	public string Name { get; init; } = "Falcons";
	public string DivisionName { get; init; } = string.Empty;
	public string ClubName { get; init; } = string.Empty;
	public string HomeArena { get; init; } = "Main Arena";
	public string PrimaryJerseyColor { get; init; } = "Red";
	public string? SecondaryJerseyColor { get; init; } = "White";
    public TeamCategory Category { get; init; } = TeamCategory.Adult;
	public List<TeamPlayerByEmailSeed> Players { get; init; } = new List<TeamPlayerByEmailSeed>();
}

public class TeamPlayerByEmailSeed
{
	public string PersonEmail { get; init; } = string.Empty;
	public FloorballPosition Position { get; init; } = FloorballPosition.Forward;
	public int JerseyNumber { get; init; } = 10;
}

public class FloorballMatchSeed
{
	public string HomeTeamName { get; init; } = string.Empty;
	public string AwayTeamName { get; init; } = string.Empty;
	public string SeasonName { get; init; } = string.Empty;
	public string ScheduledDateTime { get; init; } = string.Empty;
	public string? Venue { get; init; }
	public string? RefereeEmail { get; init; }
}

public class HockeySeasonSeed
{
	public string Name { get; init; } = "2026-27 Hockey Season";
	public string StartDate { get; init; } = "2026-09-01";
	public string EndDate { get; init; } = "2027-04-30";
	public string? SeasonCode { get; init; } = "2026-27-H";
	public TeamCategory TeamCategory { get; init; } = TeamCategory.Adult;
	public List<string> DivisionNames { get; init; } = new List<string>();
}

public class HockeyTournamentSeed
{
	public string Name { get; init; } = "Hockey Cup";
	public string StartDate { get; init; } = "2026-12-01";
	public string EndDate { get; init; } = "2026-12-15";
	public string? Venue { get; init; }
	public string? ContentHtml { get; init; }
	public bool AllGroupMatchesCompleted { get; init; } = false;
	public TeamCategory TeamCategory { get; init; } = TeamCategory.Adult;
	public List<HockeyTournamentGroupSeed> Groups { get; init; } = new List<HockeyTournamentGroupSeed>();
}

public class HockeyTournamentGroupSeed
{
	public string Name { get; init; } = "Group A";
	public List<string> TeamNames { get; init; } = new List<string>();
}

public class FootballSeasonSeed
{
	public string Name { get; init; } = "2026 Football Championship";
	public string StartDate { get; init; } = "2026-04-01";
	public string EndDate { get; init; } = "2026-10-31";
	public List<string> DivisionNames { get; init; } = new List<string>();

	public int NumberOfHalves { get; init; } = 2;
	public int HalfDurationMinutes { get; init; } = 25;
	public int PlayersOnField { get; init; } = 5;
	public bool RequireGoalkeeper { get; init; } = true;
	public int MaxSubstitutions { get; init; }
	public bool RequireOfficialsToStart { get; init; }
	public bool AllowExtraTime { get; init; }
	public int ExtraTimeHalfCount { get; init; } = 2;
	public int ExtraTimeHalfDurationMinutes { get; init; } = 15;
	public bool AllowPenaltyShootout { get; init; }
	public int WinPoints { get; init; } = 3;
	public int DrawPoints { get; init; } = 1;
	public int LossPoints { get; init; }
	public TeamCategory TeamCategory { get; init; } = TeamCategory.Adult;
}

public class FootballTournamentSeed
{
	public string Name { get; init; } = "2026 Football Cup";
	public string StartDate { get; init; } = "2026-08-01";
	public string EndDate { get; init; } = "2026-08-31";
	public string? Venue { get; init; }
	public string? ContentHtml { get; init; }

	public int GroupStageNumberOfHalves { get; init; } = 2;
	public int GroupStageHalfDurationMinutes { get; init; } = 25;
	public int GroupStagePlayersOnField { get; init; } = 5;
	public bool GroupStageRequireGoalkeeper { get; init; } = true;
	public int GroupStageMaxSubstitutions { get; init; }
	public bool GroupStageRequireOfficialsToStart { get; init; }
	public bool GroupStageAllowExtraTime { get; init; }
	public int GroupStageExtraTimeHalfCount { get; init; } = 2;
	public int GroupStageExtraTimeHalfDurationMinutes { get; init; } = 5;
	public bool GroupStageAllowPenaltyShootout { get; init; }

	public int PlayoffNumberOfHalves { get; init; } = 2;
	public int PlayoffHalfDurationMinutes { get; init; } = 25;
	public int PlayoffPlayersOnField { get; init; } = 5;
	public bool PlayoffRequireGoalkeeper { get; init; } = true;
	public int PlayoffMaxSubstitutions { get; init; }
	public bool PlayoffRequireOfficialsToStart { get; init; }
	public bool PlayoffAllowExtraTime { get; init; } = true;
	public int PlayoffExtraTimeHalfCount { get; init; } = 2;
	public int PlayoffExtraTimeHalfDurationMinutes { get; init; } = 5;
	public bool PlayoffAllowPenaltyShootout { get; init; } = true;

	public int TeamsAdvancingPerGroup { get; init; } = 2;
	public bool HasPlayoffStage { get; init; } = true;
	public bool HasThirdPlaceMatch { get; init; }
	public TeamCategory TeamCategory { get; init; } = TeamCategory.Adult;

	/// <summary>
	/// When true, the seeder transitions the tournament to GroupStage (if Draft), schedules every
	/// group-stage match in the past, and simulates each one through to completion.
	/// </summary>
	public bool AllGroupMatchesCompleted { get; init; }

	public List<FootballTournamentGroupSeed> Groups { get; init; } = new List<FootballTournamentGroupSeed>();
}

public class FootballTournamentGroupSeed
{
	public string Name { get; init; } = "Group A";
	public List<string> TeamNames { get; init; } = new List<string>();
}

public class HockeyTeamSeed
{
	public string Name { get; init; } = "Tappara HC";
	public string? ShortName { get; init; } = "TAP";
	public string DivisionName { get; init; } = string.Empty;
	public string ClubName { get; init; } = string.Empty;
	public string HomeArena { get; init; } = "Nokia Arena";
	public string PrimaryJerseyColor { get; init; } = "Blue";
	public string? SecondaryJerseyColor { get; init; } = "Orange";
	public TeamCategory Category { get; init; } = TeamCategory.Adult;
	/// <summary>Optional head coach Person email from StaffPersons.</summary>
	public string? StaffPersonEmail { get; init; }
	public List<HockeyTeamPlayerByEmailSeed> Players { get; init; } = new List<HockeyTeamPlayerByEmailSeed>();
}

public class HockeyTeamPlayerByEmailSeed
{
	public string PersonEmail { get; init; } = string.Empty;
	public HockeyPosition Position { get; init; } = HockeyPosition.Center;
	public int JerseyNumber { get; init; } = 10;
}

public class HockeyMatchSeed
{
	public string HomeTeamName { get; init; } = string.Empty;
	public string AwayTeamName { get; init; } = string.Empty;
	public string SeasonName { get; init; } = string.Empty;
	public string ScheduledDateTime { get; init; } = string.Empty;
	public string? Venue { get; init; }
	/// <summary>When true, simulate the match to Finished and recalculate stats.</summary>
	public bool SimulateCompleted { get; init; } = false;
}

public class FootballTeamSeed
{
	public string Name { get; init; } = "Helsinki FC";
	public string DivisionName { get; init; } = string.Empty;
	public string ClubName { get; init; } = string.Empty;
	public string HomeArena { get; init; } = "Main Arena";
	public string PrimaryJerseyColor { get; init; } = "Red";
	public string? SecondaryJerseyColor { get; init; } = "White";
	public TeamCategory Category { get; init; } = TeamCategory.Adult;
	public List<FootballTeamPlayerByEmailSeed> Players { get; init; } = new List<FootballTeamPlayerByEmailSeed>();
}

public class FootballTeamPlayerByEmailSeed
{
	public string PersonEmail { get; init; } = string.Empty;
	public FootballPosition Position { get; init; } = FootballPosition.Forward;
	public int JerseyNumber { get; init; } = 10;
}

public class FootballMatchSeed
{
	public string HomeTeamName { get; init; } = string.Empty;
	public string AwayTeamName { get; init; } = string.Empty;
	public string SeasonName { get; init; } = string.Empty;
	public string ScheduledDateTime { get; init; } = string.Empty;
	public string? Venue { get; init; }
	/// <summary>When true, simulate the match to Finished and recalculate stats.</summary>
	public bool SimulateCompleted { get; init; } = false;
	public string? RefereeEmail { get; init; }
}

public class LoginDevResponse
{
	public string? DevCode { get; set; }
}

public class AuthTokenResponse
{
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }
}

public static class SeederHttp
{
	public static async Task EnsureSuccessWithBody(HttpResponseMessage response, string operation)
	{
		if (response.IsSuccessStatusCode)
		{
			return;
		}

		string body = await response.Content.ReadAsStringAsync();
		throw new HttpRequestException(operation + " failed with " + (int)response.StatusCode + " " + response.StatusCode + ": " + body);
	}

    public static async Task EnsureSuccess(HttpResponseMessage response, string operation)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(operation + " failed with " + (int)response.StatusCode + " " + response.StatusCode + ": " + body);
    }
}

