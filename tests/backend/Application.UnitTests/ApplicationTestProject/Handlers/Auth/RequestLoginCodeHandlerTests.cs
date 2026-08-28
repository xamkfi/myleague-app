using Application.Common;
using Application.Configuration;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Handlers;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ApplicationTestProject.Handlers.Auth;

public class RequestLoginCodeHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserExists_UsesProviderLoginCodeMinutes()
    {
        User user = new("admin@mahl.fi", Guid.NewGuid(), UserRole.SystemAdmin);
        Mock<IUserRepository> users = new();
        Mock<IUnitOfWork> uow = new();
        Mock<IEmailService> email = new();
        Mock<ISiteSettingsProvider> provider = new();
        Mock<ILogger<RequestLoginCodeHandler>> logger = new();

        users.Setup(r => r.GetByEmailAsync("admin@mahl.fi")).ReturnsAsync(user);
        provider
            .Setup(p => p.GetEffectiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveAuthSettings(15, 7, 3, 5, 5, true));

        LoginCodeConfiguration loginCodeConfig = new()
        {
            CodeLength = 6,
            AutoFillLoginCode = true,
            ExpirationMinutes = 10
        };

        RequestLoginCodeHandler handler = new(
            users.Object,
            uow.Object,
            email.Object,
            provider.Object,
            Options.Create(loginCodeConfig),
            logger.Object);

        DateTime before = DateTime.UtcNow;
        Result<string?> result = await handler.Handle(
            new RequestLoginCodeCommand("admin@mahl.fi"),
            CancellationToken.None);
        DateTime after = DateTime.UtcNow;

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data!.Length.Should().Be(6);
        user.LoginCodeExpiresAt.Should().NotBeNull();
        user.LoginCodeExpiresAt!.Value.Should().BeOnOrAfter(before.AddMinutes(3).AddSeconds(-2));
        user.LoginCodeExpiresAt.Value.Should().BeOnOrBefore(after.AddMinutes(3).AddSeconds(2));
        users.Verify(r => r.UpdateAsync(user), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(
            e => e.SendLoginCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
