using Microsoft.Extensions.Configuration;
using Domain.Enums.Common;
using Domain.Enums.Floorball;
using Application.DTOs.Common;
using Application.DTOs.Floorball;

namespace Seeder;

public sealed class SeederConfiguration
{
	public string BaseUrl { get; set; } = "http://localhost:8080/";
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

