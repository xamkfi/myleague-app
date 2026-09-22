using Application.Features.Common.Organization.Persons.Queries;
using Application.Features.Common.Organization.Persons.Validators;
using FluentValidation.TestHelper;

namespace ApplicationTestProject.Validators.Queries.Persons;

public class GetPersonPlayerSportsQueryValidatorTests
{
    private readonly GetPersonPlayerSportsQueryValidator _validator = new();

    [Fact]
    public void Validate_ValidId_ShouldNotHaveValidationErrors()
    {
        TestValidationResult<GetPersonPlayerSportsQuery> result =
            _validator.TestValidate(new GetPersonPlayerSportsQuery(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyGuid_ShouldHaveValidationError()
    {
        TestValidationResult<GetPersonPlayerSportsQuery> result =
            _validator.TestValidate(new GetPersonPlayerSportsQuery(Guid.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
