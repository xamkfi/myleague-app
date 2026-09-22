using System.Text;
using JoomleagueImporter.Import;

namespace JoomleagueImporter.UnitTests;

public class JwtExpiryTests
{
    [Fact]
    public void ReadJwtExpiryUtc_ReadsExpClaim()
    {
        const long exp = 1_790_098_329;
        string payloadJson = "{\"exp\":" + exp + "}";
        string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        string jwt = $"header.{payload}.sig";

        DateTime expiry = ImportApiClient.ReadJwtExpiryUtc(jwt);

        expiry.Should().Be(DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime);
    }

    [Fact]
    public void ReadJwtExpiryUtc_MissingExp_UsesShortFallback()
    {
        string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"sub\":\"x\"}"))
            .TrimEnd('=');
        DateTime before = DateTime.UtcNow.AddMinutes(9);

        DateTime expiry = ImportApiClient.ReadJwtExpiryUtc($"header.{payload}.sig");

        expiry.Should().BeAfter(before);
        expiry.Should().BeBefore(DateTime.UtcNow.AddMinutes(11));
    }
}
