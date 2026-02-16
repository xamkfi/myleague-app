using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Domain.Enums.Common;
using Domain.Enums.Floorball;

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

