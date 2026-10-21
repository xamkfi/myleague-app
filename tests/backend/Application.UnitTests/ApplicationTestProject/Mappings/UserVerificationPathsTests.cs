using Application.Features.Common.Organization.Users.Mappings;
using Domain.Enums.Common;
using FluentAssertions;
using Xunit;

namespace ApplicationTestProject.Mappings;

public class UserVerificationPathsTests
{
    [Theory]
    [InlineData(UserRole.SystemAdmin, "/admin/verify-email")]
    [InlineData(UserRole.ClubAdmin, "/club-admin/verify-email")]
    [InlineData(UserRole.MatchOfficial, "/official/verify-email")]
    public void ForRole_EachRole_ReturnsThatAreasVerifyPage(UserRole role, string expected)
    {
        string path = UserVerificationPaths.ForRole(role);

        path.Should().Be(expected);
    }

    [Theory]
    [InlineData(UserRole.SystemAdmin, "/admin/login")]
    [InlineData(UserRole.ClubAdmin, "/club-admin/login")]
    [InlineData(UserRole.MatchOfficial, "/official/login")]
    public void LoginForRole_EachRole_ReturnsThatAreasLoginPage(UserRole role, string expected)
    {
        string path = UserVerificationPaths.LoginForRole(role);

        path.Should().Be(expected);
    }
}
