using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using WebAPI.Telemetry;

namespace WebApiTestProject.Telemetry;

public class UserAgentTelemetryInitializerTests
{
    private static UserAgentTelemetryInitializer CreateInitializer(HttpContext? context)
    {
        HttpContextAccessor accessor = new HttpContextAccessor { HttpContext = context };
        return new UserAgentTelemetryInitializer(accessor);
    }

    private static DefaultHttpContext ContextWithUserAgent(string? userAgent)
    {
        DefaultHttpContext context = new DefaultHttpContext();
        if (userAgent is not null)
        {
            context.Request.Headers.UserAgent = userAgent;
        }
        return context;
    }

    [Fact]
    public void Initialize_RequestWithUserAgent_CopiesHeaderToProperties()
    {
        const string userAgent = "Mozilla/5.0 (compatible; ExampleBot/1.0)";
        UserAgentTelemetryInitializer initializer = CreateInitializer(ContextWithUserAgent(userAgent));
        RequestTelemetry request = new RequestTelemetry();

        initializer.Initialize(request);

        request.Properties.Should().ContainKey(UserAgentTelemetryInitializer.PropertyName)
            .WhoseValue.Should().Be(userAgent);
    }

    [Fact]
    public void Initialize_NoHttpContext_LeavesPropertiesUntouched()
    {
        UserAgentTelemetryInitializer initializer = CreateInitializer(context: null);
        RequestTelemetry request = new RequestTelemetry();

        initializer.Initialize(request);

        request.Properties.Should().NotContainKey(UserAgentTelemetryInitializer.PropertyName);
    }

    [Fact]
    public void Initialize_MissingHeader_LeavesPropertiesUntouched()
    {
        UserAgentTelemetryInitializer initializer = CreateInitializer(ContextWithUserAgent(null));
        RequestTelemetry request = new RequestTelemetry();

        initializer.Initialize(request);

        request.Properties.Should().NotContainKey(UserAgentTelemetryInitializer.PropertyName);
    }

    [Fact]
    public void Initialize_NonRequestTelemetry_IsIgnored()
    {
        UserAgentTelemetryInitializer initializer = CreateInitializer(ContextWithUserAgent("curl/8.0"));
        TraceTelemetry trace = new TraceTelemetry("hello");

        initializer.Initialize(trace);

        trace.Properties.Should().NotContainKey(UserAgentTelemetryInitializer.PropertyName);
    }

    [Fact]
    public void Initialize_VeryLongHeader_IsTruncatedTo256Characters()
    {
        string userAgent = new string('a', 1000);
        UserAgentTelemetryInitializer initializer = CreateInitializer(ContextWithUserAgent(userAgent));
        RequestTelemetry request = new RequestTelemetry();

        initializer.Initialize(request);

        request.Properties[UserAgentTelemetryInitializer.PropertyName].Should().HaveLength(256);
    }
}
