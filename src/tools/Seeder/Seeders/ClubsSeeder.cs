using System.Net.Http.Json;
using System.Text.Json;
using Application.Features.Common.Clubs.DTOs;
using WebAPI.Models.Common;

namespace Seeder;

public static class ClubsSeeder
{
	public static async Task<List<ClubDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, SeederConfiguration config)
	{
		List<ClubDto> created = new List<ClubDto>();

		foreach (ClubSeed club in config.Clubs)
		{
            ClubDto? existingClub = await FindClubByNameAsync(http, jsonOptions, club.Name);
            if (existingClub != null)
            {
                created.Add(existingClub);
                Console.WriteLine("Club exists, skipping: " + existingClub.Name + " (" + existingClub.Id + ")");
                continue;
            }

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

	private static async Task<ClubDto?> FindClubByNameAsync(HttpClient http, JsonSerializerOptions jsonOptions, string name)
	{
		HttpResponseMessage searchResp = await http.GetAsync("api/clubs/search?name=" + Uri.EscapeDataString(name));
		if (!searchResp.IsSuccessStatusCode)
		{
			return null;
		}

		ApiResponse<List<ClubDto>>? searchApi = await searchResp.Content.ReadFromJsonAsync<ApiResponse<List<ClubDto>>>(jsonOptions);
		if (searchApi == null || !searchApi.Success || searchApi.Data == null)
		{
			return null;
		}

		return searchApi.Data.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
	}
}

