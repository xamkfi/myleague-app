using Application.Common;
using Application.Features.Auth.Commands;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Handlers;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Auth;

public class RefreshTokenHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_IssuesTokensUsingProviderValues()
    {
        User user = new("admin@mahl.fi", Guid.NewGuid(), UserRole.SystemAdmin);
        RefreshToken existing = new(user.Id, "old-hash", DateTime.UtcNow.AddDays(3));

        Mock<IUserRepository> users = new();
        Mock<IRefreshTokenRepository> tokens = new();
        Mock<IUnitOfWork> uow = new();
        Mock<IJwtTokenService> jwt = new();
        Mock<ISiteSettingsProvider> provider = new();
        Mock<ILogger<RefreshTokenHandler>> logger = new();

        tokens.Setup(r => r.GetByTokenHashAsync("old-hash")).ReturnsAsync(existing);
        users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        jwt.Setup(j => j.HashToken("refresh-raw")).Returns("old-hash");
        jwt.Setup(j => j.HashToken("new-raw")).Returns("new-hash");
        jwt.Setup(j => j.GenerateRefreshToken()).Returns("new-raw");
        DateTime accessExpires = DateTime.UtcNow.AddMinutes(7);
        jwt.Setup(j => j.GenerateAccessToken(user, 7)).Returns(("access-jwt", accessExpires));
        provider
            .Setup(p => p.GetEffectiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveAuthSettings(7, 2, 10, 5, 4, true));

        RefreshTokenHandler handler = new(
            users.Object,
            tokens.Object,
            uow.Object,
            jwt.Object,
            provider.Object,
            logger.Object);

        Result<AuthTokenDto> result = await handler.Handle(
            new RefreshTokenCommand("refresh-raw"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("access-jwt");
        result.Data.RefreshToken.Should().Be("new-raw");
        result.Data.SessionExpiryWarningMinutes.Should().Be(4);
        result.Data.ExpiresAt.Should().Be(accessExpires);
        jwt.Verify(j => j.GenerateAccessToken(user, 7), Times.Once);
        tokens.Verify(
            r => r.AddAsync(It.Is<RefreshToken>(t =>
                t.TokenHash == "new-hash"
                && t.ExpiresAt > DateTime.UtcNow.AddDays(1)
                && t.ExpiresAt < DateTime.UtcNow.AddDays(3))),
            Times.Once);
    }
}
