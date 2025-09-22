using System.Net.Http.Json;
using System.Text.Json;

namespace Seeder;

public static class ClubsSeeder
{
	public static async Task<List<ClubDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, SeederConfiguration config)
	{
		List<ClubDto> created = new List<ClubDto>();

		foreach (ClubSeed club in config.Clubs)
		{
			CreateClubRequest request = new CreateClubRequest
			{
				Name = club.Name,
				City = club.City,
				Country = club.Country,
				FoundingDate = club.FoundingDate,
				WebsiteUrl = club.WebsiteUrl ?? string.Empty,
				LogoUrl = club.LogoUrl ?? string.Empty,
				ContactEmail = club.ContactEmail ?? string.Empty
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/clubs", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Club");

			ApiResponse<ClubDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<ClubDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create club failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created club " + api.Data.Name + " (" + api.Data.Id + ")");
		}

		return created;
	}
}

