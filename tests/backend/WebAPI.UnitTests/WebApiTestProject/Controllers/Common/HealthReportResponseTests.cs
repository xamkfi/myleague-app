using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebAPI.Controllers.Health;

namespace WebApiTestProject.Controllers.Common;

public class HealthReportResponseTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Create_WithoutDetails_HidesDescriptionAndData()
    {
        HealthReport report = CreateReport();

        string json = JsonSerializer.Serialize(HealthReportResponse.Create(report, includeDetails: false), JsonOptions);

        json.Should().Contain("\"name\":\"database\"");
        json.Should().Contain("\"status\":\"Unhealthy\"");
        json.Should().NotContain("connection refused");
        json.Should().NotContain("rowCount");
    }

    [Fact]
    public void Create_WithDetails_IncludesDescriptionAndData()
    {
        HealthReport report = CreateReport();

        string json = JsonSerializer.Serialize(HealthReportResponse.Create(report, includeDetails: true), JsonOptions);

        json.Should().Contain("connection refused");
        json.Should().Contain("rowCount");
    }

    [Fact]
    public void Create_WithTag_EchoesTag()
    {
        HealthReport report = CreateReport();

        string json = JsonSerializer.Serialize(HealthReportResponse.Create(report, includeDetails: false, tag: "ready"), JsonOptions);

        json.Should().Contain("\"tag\":\"ready\"");
    }

    private static HealthReport CreateReport()
    {
        Dictionary<string, HealthReportEntry> entries = new()
        {
            ["database"] = new HealthReportEntry(
                HealthStatus.Unhealthy,
                "connection refused",
                TimeSpan.FromMilliseconds(12),
                null,
                new Dictionary<string, object> { ["rowCount"] = 42 },
                new[] { "ready" })
        };

        return new HealthReport(entries, TimeSpan.FromMilliseconds(15));
    }
}
