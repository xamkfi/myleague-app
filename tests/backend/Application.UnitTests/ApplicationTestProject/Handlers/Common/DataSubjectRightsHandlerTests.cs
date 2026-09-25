using Application.Common;
using Application.Features.Common.Organization.DataSubjectRights.Commands;
using Application.Features.Common.Organization.DataSubjectRights.DTOs;
using Application.Features.Common.Organization.DataSubjectRights.Handlers;
using Application.Features.Common.Organization.DataSubjectRights.Queries;
using Application.Features.Common.Organization.Deletion;
using Application.Features.Common.Shared.DTOs;
using Domain.Entities.Common;
using Domain.Enums.Common;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApplicationTestProject.Handlers.Common;

public class DataSubjectRightsHandlerTests
{
    private readonly Mock<IPersonRepository> _personRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPersonDeletionGuard> _deletionGuard = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public DataSubjectRightsHandlerTests()
    {
        _deletionGuard
            .Setup(x => x.EvaluateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PersonDeletionEvaluation { BlockReason = DeletionReasons.PersonHasMatchRecords });
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _userRepository.Setup(x => x.CountByRoleAsync(UserRole.SystemAdmin)).ReturnsAsync(2);
    }

    [Fact]
    public async Task Handle_GetCopy_WhenPersonMissing_ReturnsNotFound()
    {
        Guid personId = Guid.NewGuid();
        _personRepository.Setup(x => x.GetByIdAsync(personId)).ReturnsAsync((Person?)null);
        GetDataSubjectCopyHandler handler = new GetDataSubjectCopyHandler(
            _personRepository.Object,
            _userRepository.Object,
            _deletionGuard.Object,
            Mock.Of<ILogger<GetDataSubjectCopyHandler>>());

        Result<DataSubjectCopyDto> result = await handler.Handle(new GetDataSubjectCopyQuery(personId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
    }

    [Fact]
    public async Task Handle_Erase_AnonymizesAccountAndRevokesSessions()
    {
        Person person = new Person("Aino", "Aalto", new DateTime(1990, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        User user = new User("aino@example.com", person.Id, UserRole.ClubAdmin);
        user.SetLoginCode("123456", DateTime.UtcNow.AddMinutes(10));
        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _userRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(user);
        EraseDataSubjectHandler handler = new EraseDataSubjectHandler(
            _personRepository.Object,
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _deletionGuard.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<EraseDataSubjectHandler>>());

        Result<DataSubjectCopyDto> result = await handler.Handle(new EraseDataSubjectCommand(person.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.FirstName.Should().Be(Person.AnonymizedFirstName);
        result.Data.BirthDate.Should().BeNull();
        result.Data.IsProcessingRestricted.Should().BeTrue();
        result.Data.RetentionReason.Should().Be(DeletionReasons.PersonHasMatchRecords);
        result.Data.Account!.IsActive.Should().BeFalse();
        result.Data.Account.Email.Should().Be($"removed-{person.Id:N}@data-subject.invalid");
        result.Data.PortableData.AccountEmail.Should().Be(result.Data.Account.Email);
        _refreshTokenRepository.Verify(x => x.RevokeAllByUserIdAsync(user.Id), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Erase_LastSystemAdmin_DoesNotSave()
    {
        Guid personId = Guid.NewGuid();
        User user = new User("admin@example.com", personId, UserRole.SystemAdmin);
        _userRepository.Setup(x => x.GetByPersonIdAsync(personId)).ReturnsAsync(user);
        _userRepository.Setup(x => x.CountByRoleAsync(UserRole.SystemAdmin)).ReturnsAsync(1);
        EraseDataSubjectHandler handler = new EraseDataSubjectHandler(
            _personRepository.Object,
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _deletionGuard.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<EraseDataSubjectHandler>>());

        Result<DataSubjectCopyDto> result = await handler.Handle(new EraseDataSubjectCommand(personId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DeletionReasons.LastSystemAdmin);
        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(x => x.RevokeAllByUserIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Rectify_UpdatesNameAndAccountEmail()
    {
        Person person = new Person("Aino", "Aalto");
        User user = new User("aino@example.com", person.Id);
        _personRepository.Setup(x => x.GetByIdAsync(person.Id)).ReturnsAsync(person);
        _userRepository.Setup(x => x.GetByPersonIdAsync(person.Id)).ReturnsAsync(user);
        RectifyDataSubjectHandler handler = new RectifyDataSubjectHandler(
            _personRepository.Object,
            _userRepository.Object,
            _deletionGuard.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<RectifyDataSubjectHandler>>());
        RectifyDataSubjectCommand command = new RectifyDataSubjectCommand(
            person.Id,
            "Aino",
            "Korhonen",
            null,
            new AddressDto("Katu 1", null, "Mikkeli", "50100", "Finland"),
            new ContactInfoDto("aino@example.com", "0401234567", null),
            "new@example.com");

        Result<DataSubjectCopyDto> result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.LastName.Should().Be("Korhonen");
        result.Data.Address!.City.Should().Be("Mikkeli");
        result.Data.ContactInfo!.Phone.Should().Be("0401234567");
        result.Data.Account!.Email.Should().Be("new@example.com");
        result.Data.PortableData.LastName.Should().Be("Korhonen");
    }
}
