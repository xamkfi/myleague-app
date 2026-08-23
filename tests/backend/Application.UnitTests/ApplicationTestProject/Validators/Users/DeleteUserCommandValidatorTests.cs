using Application.Features.Common.Users.Commands;
using Application.Features.Common.Users.Validators;

namespace ApplicationTestProject.Validators.Users;

public class DeleteUserCommandValidatorTests
{
    private readonly DeleteUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        DeleteUserCommand command = new(Guid.NewGuid(), Guid.NewGuid());

        _validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Validate_EmptyIds_IsInvalid(bool emptyId, bool emptyRequestedBy)
    {
        DeleteUserCommand command = new(
            emptyId ? Guid.Empty : Guid.NewGuid(),
            emptyRequestedBy ? Guid.Empty : Guid.NewGuid());

        _validator.Validate(command).IsValid.Should().BeFalse();
    }
}
