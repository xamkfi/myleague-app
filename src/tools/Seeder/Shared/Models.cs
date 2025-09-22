using Microsoft.Extensions.Configuration;

namespace Seeder;

public record ApiResponse
{
	public bool Success { get; init; }
	public string Message { get; init; } = string.Empty;
	public List<string> Errors { get; init; } = new List<string>();
}

public record ApiResponse<T> : ApiResponse
{
	public T? Data { get; init; }
}

public record AddressDto
{
	public string Street1 { get; init; } = string.Empty;
	public string? Street2 { get; init; }
	public string City { get; init; } = string.Empty;
	public string PostalCode { get; init; } = string.Empty;
	public string Country { get; init; } = string.Empty;
}

public record ContactInfoDto
{
	public string Email { get; init; } = string.Empty;
	public string? Phone { get; init; }
	public string? AlternativePhone { get; init; }
}

public enum FloorballPosition
{
	None = 0,
	Forward = 1,
	Center = 2,
	Defender = 3,
	Goalkeeper = 4
}

public enum TeamCategory
{
	Adult = 0,
	Youth = 1,
	Women = 2
}

public record PersonDto(
	Guid Id,
	string FirstName,
	string LastName,
	DateTime BirthDate,
	string FullName,
	string Role,
	bool IsRegistered,
	AddressDto? Address,
	ContactInfoDto? ContactInfo);

public record ClubDto(
	Guid Id,
	string Name,
	DateTime FoundingDate,
	string City,
	string Country,
	string WebsiteUrl,
	string LogoUrl,
	string ContactEmail);

public record DivisionDto(
	Guid Id,
	string Name,
	string Description,
	int Level,
	string SportType,
	bool IsActive,
	DateTime CreatedDate);

public record FloorballPlayerDto(
	Guid Id,
	Guid PersonId,
	PersonDto Person,
	bool IsActive,
	FloorballPosition Position,
	int CareerGoals,
	int CareerAssists);

public record FloorballRefereeDto(
	Guid Id,
	Guid PersonId,
	string LicenseNumber,
	DateTime LicenseIssueDate,
	DateTime LicenseExpiryDate,
	bool IsActive,
	int MatchesOfficiated);

public record FloorballSeasonDto(
	Guid Id,
	string Name,
	Guid DivisionId,
	bool IsActive,
	DateTime StartDate,
	DateTime EndDate);

public record FloorballTeamDto(
	Guid Id,
	string Name,
	Guid DivisionId,
	Guid ClubId,
	string HomeArena,
	string PrimaryJerseyColor,
	string? SecondaryJerseyColor,
	string? LogoUrl,
	TeamCategory Category);

public record CreatePersonRequest
{
	public string FirstName { get; init; } = string.Empty;
	public string LastName { get; init; } = string.Empty;
	public string BirthDate { get; init; } = string.Empty;
	public bool IsRegistered { get; init; }
	public AddressDto? Address { get; init; }
	public ContactInfoDto? ContactInfo { get; init; }
}

public record CreateClubRequest
{
	public string Name { get; init; } = string.Empty;
	public string City { get; init; } = string.Empty;
	public string Country { get; init; } = string.Empty;
	public DateTime FoundingDate { get; init; }
	public string WebsiteUrl { get; init; } = string.Empty;
	public string LogoUrl { get; init; } = string.Empty;
	public string ContactEmail { get; init; } = string.Empty;
}

public record CreateDivisionRequest
{
	public string Name { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public int Level { get; init; }
	public string SportType { get; init; } = string.Empty;
}

public class CreateFloorballPlayerRequest
{
	public Guid PersonId { get; set; }
}

public class CreateFloorballRefereeRequest
{
	public Guid PersonId { get; set; }
	public string LicenseIssueDate { get; set; } = string.Empty;
	public string LicenseExpiryDate { get; set; } = string.Empty;
}

public class CreateFloorballSeasonRequest
{
	public string Name { get; set; } = string.Empty;
	public string StartDate { get; set; } = string.Empty;
	public string EndDate { get; set; } = string.Empty;
	public Guid DivisionId { get; set; }
}

public class FloorballTeamRequest
{
	public string Name { get; set; } = string.Empty;
	public Guid DivisionId { get; set; }
	public Guid ClubId { get; set; }
	public string HomeArena { get; set; } = string.Empty;
	public string PrimaryJerseyColor { get; set; } = string.Empty;
	public string? SecondaryJerseyColor { get; set; }
	public string? LogoUrl { get; set; }
	public TeamCategory Category { get; set; } = TeamCategory.Adult;
}

public sealed class SeederConfiguration
{
	public string BaseUrl { get; set; } = "http://localhost:8080/api/";
	public List<PersonSeed> Persons { get; set; } = new List<PersonSeed>();
	public List<ClubSeed> Clubs { get; set; } = new List<ClubSeed>();
	public List<DivisionSeed> Divisions { get; set; } = new List<DivisionSeed>();
	public List<PersonSeed> PlayerPersons { get; set; } = new List<PersonSeed>();
	public List<PersonSeed> GoaliePersons { get; set; } = new List<PersonSeed>();
	public List<PersonSeed> RefereePersons { get; set; } = new List<PersonSeed>();
	public List<FloorballSeasonSeed> FloorballSeasons { get; set; } = new List<FloorballSeasonSeed>();
	public List<FloorballTeamSeed> FloorballTeams { get; set; } = new List<FloorballTeamSeed>();

	public static SeederConfiguration Load()
	{
		IConfigurationRoot configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: true)
			.AddJsonFile("appsettings.Development.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		SeederConfiguration cfg = new SeederConfiguration();
		configuration.Bind("Seeder", cfg);

		string? envBase = configuration["Seeder:BaseUrl"];
		if (!string.IsNullOrWhiteSpace(envBase))
		{
			cfg.BaseUrl = envBase!;
		}

		string? rootBase = configuration["BaseUrl"];
		if (!string.IsNullOrWhiteSpace(rootBase))
		{
			cfg.BaseUrl = rootBase!;
		}

		string? envVar = Environment.GetEnvironmentVariable("SEEDER_BASEURL");
		if (!string.IsNullOrWhiteSpace(envVar))
		{
			cfg.BaseUrl = envVar!;
		}

		return cfg;
	}
}

public record PersonSeed
{
	public string FirstName { get; init; } = "John";
	public string LastName { get; init; } = "Doe";
	public string BirthDate { get; init; } = "1990-01-01";
	public bool IsRegistered { get; init; } = true;
	public AddressDto? Address { get; init; }
	public ContactInfoDto? ContactInfo { get; init; }
}

public record ClubSeed
{
	public string Name { get; init; } = "Sample Club";
	public string City { get; init; } = "City";
	public string Country { get; init; } = "Country";
	public DateTime FoundingDate { get; init; } = new DateTime(2000, 1, 1);
	public string? WebsiteUrl { get; init; }
	public string? LogoUrl { get; init; }
	public string? ContactEmail { get; init; }
}

public record DivisionSeed
{
	public string Name { get; init; } = "First Division";
	public string Description { get; init; } = "Top level";
	public int Level { get; init; } = 1;
	public string SportType { get; init; } = "Floorball";
}

public record FloorballSeasonSeed
{
	public string Name { get; init; } = "2025 Regular Season";
	public string StartDate { get; init; } = "2025-01-01";
	public string EndDate { get; init; } = "2025-12-31";
	public string DivisionName { get; init; } = string.Empty;
}

public record FloorballTeamSeed
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

public record TeamPlayerByEmailSeed
{
	public string PersonEmail { get; init; } = string.Empty;
	public FloorballPosition Position { get; init; } = FloorballPosition.Forward;
	public int JerseyNumber { get; init; } = 10;
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
}

