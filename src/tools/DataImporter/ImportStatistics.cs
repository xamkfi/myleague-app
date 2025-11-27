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
