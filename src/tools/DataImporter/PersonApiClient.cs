using System.Net.Http.Json;
using System.Text.Json;
using Application.DTOs.Common;
using WebAPI.Models.Common;

namespace DataImporter;

public class PersonApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public PersonApiClient(HttpClient httpClient, JsonSerializerOptions jsonOptions)
    {
        _httpClient = httpClient;
        _jsonOptions = jsonOptions;
    }

    public async Task<PersonDto?> CheckForDuplicateAsync(CreatePersonRequest request)
    {
        // Check by email if available
        if (!string.IsNullOrWhiteSpace(request.ContactInfo?.Email))
        {
            HttpResponseMessage getResp = await _httpClient.GetAsync("api/persons/by-email?email=" + Uri.EscapeDataString(request.ContactInfo.Email));
            if (getResp.IsSuccessStatusCode)
            {
                ApiResponse<PersonDto>? getApi = await getResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(_jsonOptions);
                if (getApi != null && getApi.Success && getApi.Data != null)
                {
                    return getApi.Data;
                }
            }
        }

        // Check by name + birthdate
        string fullName = (request.FirstName + " " + request.LastName).Trim();
        HttpResponseMessage searchResp = await _httpClient.GetAsync("api/persons/search?name=" + Uri.EscapeDataString(fullName));
        if (searchResp.IsSuccessStatusCode)
        {
            ApiResponse<List<PersonDto>>? searchApi = await searchResp.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>(_jsonOptions);
            if (searchApi != null && searchApi.Success && searchApi.Data != null)
            {
                foreach (PersonDto p in searchApi.Data)
                {
                    if (string.Equals(p.FirstName, request.FirstName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(p.LastName, request.LastName, StringComparison.OrdinalIgnoreCase))
                    {
                        // If birthdate matches (or both are MinValue), consider it a duplicate
                        if (DateTime.TryParse(request.BirthDate, out DateTime requestBirthDate))
                        {
                            if (p.BirthDate.Date == requestBirthDate.Date || 
                                (p.BirthDate == DateTime.MinValue && requestBirthDate == DateTime.MinValue))
                            {
                                return p;
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    public async Task<(bool Success, PersonDto? Person, string? ErrorMessage)> CreatePersonAsync(CreatePersonRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/persons", request);
        
        if (response.IsSuccessStatusCode)
        {
            ApiResponse<PersonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(_jsonOptions);
            if (api != null && api.Success && api.Data != null)
            {
                return (true, api.Data, null);
            }
            else
            {
                return (false, null, "Invalid response from server");
            }
        }
        else
        {
            string body = await response.Content.ReadAsStringAsync();
            return (false, null, $"{response.StatusCode} - {body}");
        }
    }
}

