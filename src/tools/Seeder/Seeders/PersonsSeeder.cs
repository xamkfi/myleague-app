using System.Net.Http.Json;
using System.Text.Json;

namespace Seeder;

public static class PersonsSeeder
{
	public static async Task<List<PersonDto>> SeedAsync(HttpClient http, JsonSerializerOptions jsonOptions, SeederConfiguration config)
	{
		return await SeedListAsync(http, jsonOptions, config.Persons);
	}

	public static async Task<List<PersonDto>> SeedListAsync(HttpClient http, JsonSerializerOptions jsonOptions, List<PersonSeed> persons)
	{
		List<PersonDto> created = new List<PersonDto>();

		foreach (PersonSeed person in persons)
		{
			CreatePersonRequest request = new CreatePersonRequest
			{
				FirstName = person.FirstName,
				LastName = person.LastName,
				BirthDate = person.BirthDate,
				IsRegistered = person.IsRegistered,
				Address = person.Address,
				ContactInfo = person.ContactInfo
			};

			HttpResponseMessage response = await http.PostAsJsonAsync("api/persons", request);
			await SeederHttp.EnsureSuccessWithBody(response, "Create Person");

			ApiResponse<PersonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
			if (api == null || !api.Success || api.Data == null)
			{
				throw new InvalidOperationException("Create person failed: " + (api != null ? api.Message : "null response"));
			}

			created.Add(api.Data);
			Console.WriteLine("Created person " + api.Data.FullName + " (" + api.Data.Id + ")");
		}

		return created;
	}
}

