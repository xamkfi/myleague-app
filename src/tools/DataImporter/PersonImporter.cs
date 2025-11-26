using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using Application.DTOs.Common;
using WebAPI.Models.Common;

namespace DataImporter;

public class ImportStatistics
{
    public int TotalProcessed { get; set; }
    public int Created { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> CreatedPersonNames { get; set; } = new();
    public List<string> DuplicatePersonNames { get; set; } = new();
    public List<(string Name, string Error)> FailedPersons { get; set; } = new();
    public List<string> SkippedPersonNames { get; set; } = new(); // For missing data
}

public static class PersonImporter
{
    public static async Task<ImportStatistics> ImportFromJlgFilesAsync(HttpClient http, JsonSerializerOptions jsonOptions, string dataFilesPath)
    {
        ImportStatistics stats = new ImportStatistics();

        // Get all .jlg files from DataFiles folder
        string[] jlgFiles = Directory.GetFiles(dataFilesPath, "*.jlg", SearchOption.TopDirectoryOnly);

        if (jlgFiles.Length == 0)
        {
            Console.WriteLine($"No .jlg files found in {dataFilesPath}");
            return stats;
        }

        Console.WriteLine($"Found {jlgFiles.Length} .jlg file(s) to process");

        foreach (string filePath in jlgFiles)
        {
            Console.WriteLine($"\nProcessing file: {Path.GetFileName(filePath)}");
            ImportStatistics fileStats = await ProcessJlgFileAsync(http, jsonOptions, filePath);
            
            stats.TotalProcessed += fileStats.TotalProcessed;
            stats.Created += fileStats.Created;
            stats.Skipped += fileStats.Skipped;
            stats.Failed += fileStats.Failed;
            stats.Errors.AddRange(fileStats.Errors);
            stats.CreatedPersonNames.AddRange(fileStats.CreatedPersonNames);
            stats.DuplicatePersonNames.AddRange(fileStats.DuplicatePersonNames);
            stats.FailedPersons.AddRange(fileStats.FailedPersons);
            stats.SkippedPersonNames.AddRange(fileStats.SkippedPersonNames);
        }

        return stats;
    }

    private static async Task<ImportStatistics> ProcessJlgFileAsync(HttpClient http, JsonSerializerOptions jsonOptions, string filePath)
    {
        ImportStatistics stats = new ImportStatistics();

        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNodeList? personRecords = doc.SelectNodes("//record[@object='Person']");
            
            if (personRecords == null || personRecords.Count == 0)
            {
                Console.WriteLine($"  No Person records found in {Path.GetFileName(filePath)}");
                return stats;
            }

            Console.WriteLine($"  Found {personRecords.Count} Person record(s)");

            foreach (XmlNode personNode in personRecords)
            {
                stats.TotalProcessed++;
                XmlPersonData? xmlPerson = null;

                try
                {
                    xmlPerson = ParsePersonNode(personNode);
                    
                    // Skip if firstname or lastname is empty
                    if (string.IsNullOrWhiteSpace(xmlPerson.FirstName) || string.IsNullOrWhiteSpace(xmlPerson.LastName))
                    {
                        stats.Skipped++;
                        string skippedName = $"{xmlPerson.FirstName} {xmlPerson.LastName}".Trim();
                        if (string.IsNullOrWhiteSpace(skippedName))
                            skippedName = "[Missing Name]";
                        stats.SkippedPersonNames.Add(skippedName);
                        Console.WriteLine($"  Skipped person: Missing firstname or lastname - {skippedName}");
                        continue;
                    }

                    CreatePersonRequest request = MapToCreatePersonRequest(xmlPerson);

                    // Check for duplicates
                    PersonDto? existing = await CheckForDuplicateAsync(http, jsonOptions, request);
                    
                    if (existing != null)
                    {
                        stats.Skipped++;
                        string duplicateName = $"{request.FirstName} {request.LastName}".Trim();
                        stats.DuplicatePersonNames.Add(duplicateName);
                        Console.WriteLine($"  Person exists, skipping: {existing.FullName} ({existing.Id})");
                        continue;
                    }

                    // Create person
                    HttpResponseMessage response = await http.PostAsJsonAsync("api/persons", request);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        ApiResponse<PersonDto>? api = await response.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
                        if (api != null && api.Success && api.Data != null)
                        {
                            stats.Created++;
                            string fullName = api.Data.FullName;
                            stats.CreatedPersonNames.Add(fullName);
                            Console.WriteLine($"  Created person: {fullName} ({api.Data.Id})");
                        }
                        else
                        {
                            stats.Failed++;
                            string failedName = $"{xmlPerson.FirstName} {xmlPerson.LastName}".Trim();
                            string errorMsg = "Invalid response from server";
                            stats.FailedPersons.Add((failedName, errorMsg));
                            string fullErrorMsg = $"Failed to create person {failedName}: {errorMsg}";
                            stats.Errors.Add(fullErrorMsg);
                            Console.WriteLine($"  {fullErrorMsg}");
                        }
                    }
                    else
                    {
                        stats.Failed++;
                        string failedName = $"{xmlPerson.FirstName} {xmlPerson.LastName}".Trim();
                        string body = await response.Content.ReadAsStringAsync();
                        string errorMsg = $"{response.StatusCode} - {body}";
                        stats.FailedPersons.Add((failedName, errorMsg));
                        string fullErrorMsg = $"Failed to create person {failedName}: {errorMsg}";
                        stats.Errors.Add(fullErrorMsg);
                        Console.WriteLine($"  {fullErrorMsg}");
                    }
                }
                catch (Exception ex)
                {
                    stats.Failed++;
                    string failedName = xmlPerson != null 
                        ? $"{xmlPerson.FirstName} {xmlPerson.LastName}".Trim() 
                        : "[Unknown Name]";
                    if (string.IsNullOrWhiteSpace(failedName))
                        failedName = "[Unknown Name]";
                    string errorMsg = ex.Message;
                    stats.FailedPersons.Add((failedName, errorMsg));
                    string fullErrorMsg = $"Error processing person record ({failedName}): {errorMsg}";
                    stats.Errors.Add(fullErrorMsg);
                    Console.WriteLine($"  {fullErrorMsg}");
                }
            }
        }
        catch (Exception ex)
        {
            string errorMsg = $"Error reading file {Path.GetFileName(filePath)}: {ex.Message}";
            stats.Errors.Add(errorMsg);
            Console.WriteLine($"  {errorMsg}");
        }

        return stats;
    }

    private static XmlPersonData ParsePersonNode(XmlNode personNode)
    {
        XmlPersonData person = new XmlPersonData();

        person.FirstName = GetCDataValue(personNode, "firstname") ?? string.Empty;
        person.LastName = GetCDataValue(personNode, "lastname") ?? string.Empty;
        person.Birthday = GetCDataValue(personNode, "birthday") ?? string.Empty;
        person.Country = GetCDataValue(personNode, "country") ?? string.Empty;
        person.Email = GetCDataValue(personNode, "email") ?? string.Empty;
        person.Phone = GetCDataValue(personNode, "phone") ?? string.Empty;
        person.Mobile = GetCDataValue(personNode, "mobile") ?? string.Empty;
        person.Address = GetCDataValue(personNode, "address") ?? string.Empty;
        person.ZipCode = GetCDataValue(personNode, "zipcode") ?? string.Empty;
        person.Location = GetCDataValue(personNode, "location") ?? string.Empty;
        person.State = GetCDataValue(personNode, "state") ?? string.Empty;
        person.AddressCountry = GetCDataValue(personNode, "address_country") ?? string.Empty;

        return person;
    }

    private static string? GetCDataValue(XmlNode parentNode, string elementName)
    {
        XmlNode? node = parentNode.SelectSingleNode(elementName);
        if (node != null && node.FirstChild is XmlCDataSection cdata)
        {
            return cdata.Value;
        }
        return null;
    }

    private static CreatePersonRequest MapToCreatePersonRequest(XmlPersonData xmlPerson)
    {
        // Parse birthdate - handle "0000-00-00" as DateTime.MinValue
        string birthDateStr = ParseBirthDate(xmlPerson.Birthday);

        // Determine IsRegistered based on country code "AFG"
        bool isRegistered = xmlPerson.Country == "AFG";

        // Map phone/mobile - use non-empty as Phone, other as AlternativePhone
        string? phone = GetNonEmptyPhone(xmlPerson.Phone, xmlPerson.Mobile);
        string? alternativePhone = GetAlternativePhone(xmlPerson.Phone, xmlPerson.Mobile);

        // Create AddressDto if we have any address data
        AddressDto? address = null;
        if (!string.IsNullOrWhiteSpace(xmlPerson.Address) ||
            !string.IsNullOrWhiteSpace(xmlPerson.Location) ||
            !string.IsNullOrWhiteSpace(xmlPerson.ZipCode) ||
            !string.IsNullOrWhiteSpace(xmlPerson.AddressCountry))
        {
            address = new AddressDto(
                xmlPerson.Address ?? string.Empty,
                xmlPerson.State ?? string.Empty,
                xmlPerson.Location ?? string.Empty,
                xmlPerson.ZipCode ?? string.Empty,
                xmlPerson.AddressCountry ?? string.Empty
            );
        }

        // Create ContactInfoDto if we have any contact data
        ContactInfoDto? contactInfo = null;
        if (!string.IsNullOrWhiteSpace(xmlPerson.Email) ||
            !string.IsNullOrWhiteSpace(phone) ||
            !string.IsNullOrWhiteSpace(alternativePhone))
        {
            contactInfo = new ContactInfoDto(
                xmlPerson.Email ?? string.Empty,
                phone ?? string.Empty,
                alternativePhone ?? string.Empty
            );
        }

        return new CreatePersonRequest
        {
            FirstName = xmlPerson.FirstName,
            LastName = xmlPerson.LastName,
            BirthDate = birthDateStr,
            IsRegistered = isRegistered,
            Address = address,
            ContactInfo = contactInfo
        };
    }

    private static string ParseBirthDate(string birthday)
    {
        if (string.IsNullOrWhiteSpace(birthday) || birthday == "0000-00-00")
        {
            return DateTime.MinValue.ToString("yyyy-MM-dd");
        }

        // Try to parse the date
        if (DateTime.TryParse(birthday, out DateTime parsedDate))
        {
            return parsedDate.ToString("yyyy-MM-dd");
        }

        // If parsing fails, return MinValue
        return DateTime.MinValue.ToString("yyyy-MM-dd");
    }

    private static string? GetNonEmptyPhone(string? phone, string? mobile)
    {
        if (!string.IsNullOrWhiteSpace(phone))
            return phone;
        if (!string.IsNullOrWhiteSpace(mobile))
            return mobile;
        return null;
    }

    private static string? GetAlternativePhone(string? phone, string? mobile)
    {
        if (!string.IsNullOrWhiteSpace(phone) && !string.IsNullOrWhiteSpace(mobile))
            return mobile;
        return null;
    }

    private static async Task<PersonDto?> CheckForDuplicateAsync(HttpClient http, JsonSerializerOptions jsonOptions, CreatePersonRequest request)
    {
        // Check by email if available
        if (!string.IsNullOrWhiteSpace(request.ContactInfo?.Email))
        {
            HttpResponseMessage getResp = await http.GetAsync("api/persons/by-email?email=" + Uri.EscapeDataString(request.ContactInfo.Email));
            if (getResp.IsSuccessStatusCode)
            {
                ApiResponse<PersonDto>? getApi = await getResp.Content.ReadFromJsonAsync<ApiResponse<PersonDto>>(jsonOptions);
                if (getApi != null && getApi.Success && getApi.Data != null)
                {
                    return getApi.Data;
                }
            }
        }

        // Check by name + birthdate
        string fullName = (request.FirstName + " " + request.LastName).Trim();
        HttpResponseMessage searchResp = await http.GetAsync("api/persons/search?name=" + Uri.EscapeDataString(fullName));
        if (searchResp.IsSuccessStatusCode)
        {
            ApiResponse<List<PersonDto>>? searchApi = await searchResp.Content.ReadFromJsonAsync<ApiResponse<List<PersonDto>>>(jsonOptions);
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

    private class XmlPersonData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Birthday { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string AddressCountry { get; set; } = string.Empty;
    }
}
