using Application.Common;
using Application.Features.Common.Deletion;
using Application.Features.Common.Users.Commands;
using Application.Features.Common.Users.Handlers;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Users;

public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteUserHandler _handler;

    public DeleteUserHandlerTests()
    {
        _handler = new DeleteUserHandler(
            _userRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<DeleteUserHandler>>());
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        Guid userId = Guid.NewGuid();
        _userRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        Result<bool> result = await _handler.Handle(
            new DeleteUserCommand(userId, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _userRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SelfDelete_ReturnsFailure()
    {
        User user = new User("admin@test.com", Guid.NewGuid(), UserRole.SystemAdmin);
        _userRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

        Result<bool> result = await _handler.Handle(
            new DeleteUserCommand(user.Id, user.Id),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.CannotDeleteOwnAccount);
        _userRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_LastSystemAdmin_ReturnsFailure()
    {
        User user = new User("admin@test.com", Guid.NewGuid(), UserRole.SystemAdmin);
        Guid requestedBy = Guid.NewGuid();
        _userRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(x => x.CountByRoleAsync(UserRole.SystemAdmin)).ReturnsAsync(1);

        Result<bool> result = await _handler.Handle(
            new DeleteUserCommand(user.Id, requestedBy),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeletionReasons.LastSystemAdmin);
        _userRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OtherAdminWhenMultipleExist_Deletes()
    {
        User user = new User("admin@test.com", Guid.NewGuid(), UserRole.SystemAdmin);
        Guid requestedBy = Guid.NewGuid();
        _userRepository.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userRepository.Setup(x => x.CountByRoleAsync(UserRole.SystemAdmin)).ReturnsAsync(2);

        Result<bool> result = await _handler.Handle(
            new DeleteUserCommand(user.Id, requestedBy),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userRepository.Verify(x => x.DeleteAsync(user.Id), Times.Once);
    }
}
