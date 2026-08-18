using System.Text.Json;

namespace JoomleagueImporter.Import;

public class ImportLogger : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly string _logPath;
    private int _errorCount;

    public int ErrorCount => _errorCount;
    public string LogPath => _logPath;

    public ImportLogger(string logDirectory)
    {
        Directory.CreateDirectory(logDirectory);
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _logPath = Path.Combine(logDirectory, $"import_errors_{timestamp}.log");
        _writer = new StreamWriter(_logPath, append: false) { AutoFlush = true };
        _writer.WriteLine($"JoomLeague Import Error Log - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        _writer.WriteLine(new string('=', 80));
        _writer.WriteLine();
    }

    public void LogError(string operation, object? input, string errorMessage)
    {
        _errorCount++;
        _writer.WriteLine($"[ERROR #{_errorCount}] {DateTime.Now:HH:mm:ss} - {operation}");
        _writer.WriteLine($"  Error: {errorMessage}");
        if (input != null)
        {
            try
            {
                string json = JsonSerializer.Serialize(input, new JsonSerializerOptions { WriteIndented = true });
                _writer.WriteLine($"  Input: {json}");
            }
            catch
            {
                _writer.WriteLine($"  Input: {input}");
            }
        }
        _writer.WriteLine();
    }

    public void LogWarning(string operation, string message)
    {
        _writer.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} - {operation}: {message}");
        _writer.WriteLine();
    }

    public void LogInfo(string message)
    {
        _writer.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
    }

    public void Dispose()
    {
        _writer.WriteLine();
        _writer.WriteLine(new string('=', 80));
        _writer.WriteLine($"Total errors: {_errorCount}");
        _writer.Dispose();
    }
}
