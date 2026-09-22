using Application.Common;
using Microsoft.AspNetCore.Http;
using WebAPI.Controllers.Common;

namespace WebApiTestProject.Controllers.Common;

public class ApiErrorHttpMapperTests
{
    [Fact]
    public void Map_NotFoundKindWithoutNotFoundText_Returns404()
    {
        Result<string> result = Result<string>.NotFound("Club", "abc");

        ApiErrorHttpDecision decision = ApiErrorHttpMapper.Map(
            ResultErrorKind.NotFound,
            "Missing club",
            Array.Empty<string>(),
            "Failed",
            isDevelopment: false);

        result.ErrorKind.Should().Be(ResultErrorKind.NotFound);
        decision.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        decision.Message.Should().Be("Missing club");
    }

    [Fact]
    public void Map_ValidationKindWithNotFoundText_Returns400()
    {
        Result<string> result = Result<string>.ValidationFailure(new[] { "Player was not found" });

        ApiErrorHttpDecision decision = ApiErrorHttpMapper.Map(
            result.ErrorKind,
            "Player was not found",
            result.GetAllErrors(),
            "Failed",
            isDevelopment: false);

        result.ErrorKind.Should().Be(ResultErrorKind.Validation);
        decision.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        decision.Message.Should().Be("Player was not found");
    }

    [Fact]
    public void Map_InfrastructureExceptionOutsideDevelopment_ReturnsGeneric500()
    {
        const string leaked = "PostgresException: duplicate key value violates unique constraint";
        Result<string> result = Result<string>.Failure(
            "23505: duplicate key value violates unique constraint",
            new[] { leaked, "DbUpdateException: An error occurred while saving the entity changes." });

        ApiErrorHttpDecision decision = ApiErrorHttpMapper.Map(
            result.ErrorKind,
            result.Error,
            result.GetAllErrors(),
            "Failed",
            isDevelopment: false);

        decision.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        decision.Message.Should().Be(ApiErrorHttpMapper.InternalServerErrorMessage);
        string body = decision.Message + " " + string.Join(" ", decision.Errors);
        body.Should().NotContain("PostgresException");
        body.Should().NotContain("duplicate key");
        body.Should().NotContain("DbUpdateException");
    }

    [Fact]
    public void Map_InfrastructureExceptionInDevelopment_PreservesFlattenText()
    {
        const string leaked = "PostgresException: duplicate key value violates unique constraint";
        Result<string> result = Result<string>.Failure("duplicate key", new[] { leaked });

        ApiErrorHttpDecision decision = ApiErrorHttpMapper.Map(
            result.ErrorKind,
            result.Error,
            result.GetAllErrors(),
            "Failed",
            isDevelopment: true);

        decision.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        decision.Message.Should().Be("duplicate key");
        decision.Errors.Should().Contain(leaked);
    }

    [Fact]
    public void Map_FailureMessageWasNotFound_Returns404()
    {
        Result<string> result = Result<string>.Failure("Club with key 'abc' was not found.");

        ApiErrorHttpDecision decision = ApiErrorHttpMapper.Map(
            result.ErrorKind,
            result.Error,
            result.GetAllErrors(),
            "Failed",
            isDevelopment: false);

        result.ErrorKind.Should().Be(ResultErrorKind.Failure);
        decision.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        decision.Message.Should().Be("Club with key 'abc' was not found.");
    }

    [Fact]
    public void Map_DomainInvalidOperationException_Stays400()
    {
        const string domainMessage = "Match is not in progress.";
        Result<string> result = Result<string>.Failure(
            domainMessage,
            new[] { "InvalidOperationException: Match is not in progress." });

        ApiErrorHttpDecision decision = ApiErrorHttpMapper.Map(
            result.ErrorKind,
            result.Error,
            result.GetAllErrors(),
            "Failed",
            isDevelopment: false);

        decision.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        decision.Message.Should().Be(domainMessage);
        decision.Errors.Should().Contain("InvalidOperationException: Match is not in progress.");
    }
}
