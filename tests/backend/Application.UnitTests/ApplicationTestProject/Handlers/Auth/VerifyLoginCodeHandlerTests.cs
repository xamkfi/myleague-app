using Application.Common;
using Application.Configuration;
using Application.Features.Auth.Commands;
using Application.Features.Auth.DTOs;
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

public class VerifyLoginCodeHandlerTests
{
    private const string Email = "admin@mahl.fi";
    private const string Code = "123456";

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokens = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();
    private readonly Mock<ISiteSettingsProvider> _siteSettings = new();
    private readonly User _user = new(Email, Guid.NewGuid(), UserRole.SystemAdmin);

    public VerifyLoginCodeHandlerTests()
    {
        _jwtTokenService
            .Setup(s => s.HashToken(It.IsAny<string>()))
            .Returns((string token) => $"hash:{token}");
        _jwtTokenService
            .Setup(s => s.GenerateAccessToken(It.IsAny<User>(), It.IsAny<int>()))
            .Returns(("access-token", DateTime.UtcNow.AddMinutes(15)));
        _jwtTokenService.Setup(s => s.GenerateRefreshToken()).Returns("refresh-token");
        _siteSettings
            .Setup(p => p.GetEffectiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveAuthSettings(15, 7, 3, 5, 5, true));
        _users.Setup(r => r.GetByEmailAsync(Email)).ReturnsAsync(_user);
        _user.SetLoginCode($"hash:{Code}", DateTime.UtcNow.AddMinutes(5));
    }

    [Fact]
    public async Task Handle_WithMatchingCode_ReturnsTokens()
    {
        AllowAttempt(true);

        Result<AuthTokenDto> result = await CreateHandler().Handle(
            new VerifyLoginCodeCommand(Email, Code),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("access-token");
        _refreshTokens.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithWrongCode_FailsWithoutIssuingTokens()
    {
        AllowAttempt(true);

        Result<AuthTokenDto> result = await CreateHandler().Handle(
            new VerifyLoginCodeCommand(Email, "654321"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid login code.");
        _jwtTokenService.Verify(s => s.GenerateAccessToken(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAttemptLimitReached_ClearsCodeAndRejectsEvenCorrectCode()
    {
        AllowAttempt(false);

        Result<AuthTokenDto> result = await CreateHandler().Handle(
            new VerifyLoginCodeCommand(Email, Code),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().StartWith("Too many failed attempts");
        _user.LoginCode.Should().BeNull();
        _jwtTokenService.Verify(s => s.GenerateAccessToken(It.IsAny<User>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsGenericError()
    {
        _users.Setup(r => r.GetByEmailAsync("missing@mahl.fi")).ReturnsAsync((User?)null);

        Result<AuthTokenDto> result = await CreateHandler().Handle(
            new VerifyLoginCodeCommand("missing@mahl.fi", Code),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or login code.");
    }

    private void AllowAttempt(bool allowed)
    {
        _users
            .Setup(r => r.TryConsumeLoginAttemptAsync(_user.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(allowed);
    }

    private VerifyLoginCodeHandler CreateHandler()
    {
        return new VerifyLoginCodeHandler(
            _users.Object,
            _refreshTokens.Object,
            _unitOfWork.Object,
            _jwtTokenService.Object,
            _siteSettings.Object,
            Options.Create(new LoginCodeConfiguration { CodeLength = 6 }),
            new Mock<ILogger<VerifyLoginCodeHandler>>().Object);
    }
}
