using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Common;
using WebAPI.Models.Common;

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
            // Idempotent check by email if available, else by name + birthdate
            PersonDto? existing = null;
            string? email = person.ContactInfo != null ? person.ContactInfo.Email : null;
            if (!string.IsNullOrWhiteSpace(email))
            {
                HttpResponseMessage getResp = await http.GetAsync("api/persons/by-email?email=" + Uri.EscapeDataString(email!));
                if (getResp.IsSuccessStatusCode)
                {
                    ApiResponse<PersonDto>? getApi = await getResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
                    if (getApi != null && getApi.Success && getApi.Data != null)
                    {
                        existing = getApi.Data;
                    }
                }
            }
            if (existing == null)
            {
                // try search by full name
                string fullName = (person.FirstName + " " + person.LastName).Trim();
                HttpResponseMessage searchResp = await http.GetAsync("api/persons/search?name=" + Uri.EscapeDataString(fullName));
                if (searchResp.IsSuccessStatusCode)
                {
                    ApiResponse<List<PersonDto>>? searchApi = await searchResp.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>(jsonOptions);
                    if (searchApi != null && searchApi.Success && searchApi.Data != null)
                    {
                        foreach (PersonDto p in searchApi.Data)
                        {
                            if (string.Equals(p.FirstName, person.FirstName, StringComparison.OrdinalIgnoreCase)
                                && string.Equals(p.LastName, person.LastName, StringComparison.OrdinalIgnoreCase)
                                && p.BirthDate.Date == DateTime.Parse(person.BirthDate).Date)
                            {
                                existing = p;
                                break;
                            }
                        }
                    }
                }
            }

            if (existing != null)
            {
                created.Add(existing);
                Console.WriteLine("Person exists, skipping: " + existing.FullName + " (" + existing.Id + ")");
                continue;
            }

            CreatePersonRequest request = new CreatePersonRequest
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                BirthDate = person.BirthDate,
                IsRegistered = person.IsRegistered,
                Address = ToAddressDto(person.Address),
                ContactInfo = ToContactInfoDto(person.ContactInfo)
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

    private static AddressDto? ToAddressDto(AddressSeed? seed)
    {
        if (seed == null)
        {
            return null;
        }
        string country = seed.Country ?? string.Empty;
        return new AddressDto(
            seed.Street1 ?? string.Empty,
            seed.Street2 ?? string.Empty,
            seed.City ?? string.Empty,
            seed.PostalCode ?? string.Empty,
            country
        );
    }

    private static ContactInfoDto? ToContactInfoDto(ContactInfoSeed? seed)
    {
        if (seed == null)
        {
            return null;
        }
        return new ContactInfoDto(
            seed.Email ?? string.Empty,
            seed.Phone ?? string.Empty,
            seed.AlternativePhone ?? string.Empty
        );
    }
}

