using System.Text.Json;
using Application.DTOs.Common;
using DataImporter.Models;
using WebAPI.Models.Common;

namespace DataImporter;

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

        PersonApiClient apiClient = new PersonApiClient(http, jsonOptions);

        foreach (string filePath in jlgFiles)
        {
            Console.WriteLine($"\nProcessing file: {Path.GetFileName(filePath)}");
            ImportStatistics fileStats = await ProcessJlgFileAsync(apiClient, filePath);
            
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

    private static async Task<ImportStatistics> ProcessJlgFileAsync(PersonApiClient apiClient, string filePath)
    {
        ImportStatistics stats = new ImportStatistics();

        try
        {
            List<XmlPersonData> xmlPersons = JlgFileReader.ReadJlgFile(filePath);
            
            if (xmlPersons.Count == 0)
            {
                Console.WriteLine($"  No Person records found in {Path.GetFileName(filePath)}");
                return stats;
            }

            Console.WriteLine($"  Found {xmlPersons.Count} Person record(s)");

            foreach (XmlPersonData xmlPerson in xmlPersons)
            {
                stats.TotalProcessed++;

                try
                {
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

                    CreatePersonRequest request = PersonMapper.MapToCreatePersonRequest(xmlPerson);

                    // Check for duplicates
                    PersonDto? existing = await apiClient.CheckForDuplicateAsync(request);
                    
                    if (existing != null)
                    {
                        stats.Skipped++;
                        string duplicateName = $"{request.FirstName} {request.LastName}".Trim();
                        stats.DuplicatePersonNames.Add(duplicateName);
                        Console.WriteLine($"  Person exists, skipping: {existing.FullName} ({existing.Id})");
                        continue;
                    }

                    // Create person
                    (bool success, PersonDto? person, string? errorMessage) = await apiClient.CreatePersonAsync(request);
                    
                    if (success && person != null)
                    {
                        stats.Created++;
                        string fullName = person.FullName;
                        stats.CreatedPersonNames.Add(fullName);
                        Console.WriteLine($"  Created person: {fullName} ({person.Id})");
                    }
                    else
                    {
                        stats.Failed++;
                        string failedName = $"{xmlPerson.FirstName} {xmlPerson.LastName}".Trim();
                        string errorMsg = errorMessage ?? "Unknown error";
                        stats.FailedPersons.Add((failedName, errorMsg));
                        string fullErrorMsg = $"Failed to create person {failedName}: {errorMsg}";
                        stats.Errors.Add(fullErrorMsg);
                        Console.WriteLine($"  {fullErrorMsg}");
                    }
                }
                catch (Exception ex)
                {
                    stats.Failed++;
                    string failedName = $"{xmlPerson.FirstName} {xmlPerson.LastName}".Trim();
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
}
