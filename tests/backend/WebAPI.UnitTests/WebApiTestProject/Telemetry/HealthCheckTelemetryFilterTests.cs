using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using WebAPI.Telemetry;

namespace WebApiTestProject.Telemetry;

public class HealthCheckTelemetryFilterTests
{
    private sealed class RecordingProcessor : ITelemetryProcessor
    {
        public List<ITelemetry> Items { get; } = new();

        public void Process(ITelemetry item)
        {
            Items.Add(item);
        }
    }

    private static RequestTelemetry Request(string url, bool? success)
    {
        RequestTelemetry request = new RequestTelemetry
        {
            Name = "GET " + url,
            Url = new Uri("https://mahl-prod-api.azurewebsites.net" + url),
            Success = success
        };
        return request;
    }

    [Fact]
    public void Process_SuccessfulHealthReadyRequest_IsDropped()
    {
        RecordingProcessor next = new RecordingProcessor();
        HealthCheckTelemetryFilter filter = new HealthCheckTelemetryFilter(next);

        filter.Process(Request("/health/ready", success: true));

        next.Items.Should().BeEmpty("successful health probes are not worth storing");
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    [InlineData("/HEALTH/READY")]
    public void ShouldDrop_SuccessfulHealthPaths_ReturnsTrue(string path)
    {
        HealthCheckTelemetryFilter.ShouldDrop(Request(path, success: true)).Should().BeTrue();
    }

    [Fact]
    public void Process_FailedHealthRequest_IsKept()
    {
        RecordingProcessor next = new RecordingProcessor();
        HealthCheckTelemetryFilter filter = new HealthCheckTelemetryFilter(next);

        filter.Process(Request("/health/ready", success: false));

        next.Items.Should().HaveCount(1, "a failing probe is the signal we want to keep");
    }

    [Fact]
    public void Process_ApiRequest_IsKept()
    {
        RecordingProcessor next = new RecordingProcessor();
        HealthCheckTelemetryFilter filter = new HealthCheckTelemetryFilter(next);

        filter.Process(Request("/api/FooterContact", success: true));

        next.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Process_NonRequestTelemetry_IsKept()
    {
        RecordingProcessor next = new RecordingProcessor();
        HealthCheckTelemetryFilter filter = new HealthCheckTelemetryFilter(next);

        filter.Process(new TraceTelemetry("health check passed"));

        next.Items.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldDrop_RequestWithoutUrl_ReturnsFalse()
    {
        RequestTelemetry request = new RequestTelemetry { Name = "GET /health/ready", Success = true };

        HealthCheckTelemetryFilter.ShouldDrop(request).Should().BeFalse();
    }
}
