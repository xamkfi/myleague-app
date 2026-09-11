using Application.Features.Common.Clubs.Commands;
using Application.Features.Common.Clubs.Validators;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace ApplicationTestProject.Validators.Commands.Club;

public class CreateClubCommandValidatorTests
{
    private readonly CreateClubCommandValidator _validator;

    public CreateClubCommandValidatorTests()
    {
        _validator = new CreateClubCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club Name",
            "Valid City",
            "Valid Country",
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "https://validclub.com",
            "https://validclub.com/logo.png",
            "contact@validclub.com"
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidCommandWithMinimalData_ShouldNotHaveValidationErrors()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_EmptyOrNullName_ShouldHaveValidationError(string name)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            name,
            null,
            null,
            null
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Club name is required");
    }

    [Fact]
    public void Validate_NameTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longName = new string('A', 101); // 101 characters
        CreateClubCommand command = new CreateClubCommand(
            longName,
            null,
            null,
            null
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Club name cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_NameExactlyMaxLength_ShouldNotHaveValidationError()
    {
        // Arrange
        string maxLengthName = new string('A', 100); // Exactly 100 characters
        CreateClubCommand command = new CreateClubCommand(
            maxLengthName,
            null,
            null,
            null
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_CityTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longCity = new string('B', 51); // 51 characters
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            longCity,
            null,
            null
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorMessage("City cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_CountryTooLong_ShouldHaveValidationError()
    {
        // Arrange
        string longCountry = new string('C', 51); // 51 characters
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            longCountry,
            null
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Country)
            .WithErrorMessage("Country cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_FutureFoundingDate_ShouldHaveValidationError()
    {
        // Arrange
        DateTime futureDate = DateTime.UtcNow.AddDays(1);
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            futureDate
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FoundingDate)
            .WithErrorMessage("Founding date cannot be in the future");
    }

    [Fact]
    public void Validate_PastFoundingDate_ShouldNotHaveValidationError()
    {
        // Arrange
        DateTime pastDate = DateTime.UtcNow.AddDays(-1); // Use past date to avoid timing issues
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            pastDate
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FoundingDate);
    }

    [Theory]
    [InlineData("https://validurl.com")]
    [InlineData("http://validurl.com")]
    [InlineData("https://subdomain.validurl.com/path")]
    [InlineData("http://localhost:8080")]
    public void Validate_ValidWebsiteUrl_ShouldNotHaveValidationError(string websiteUrl)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            websiteUrl
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WebsiteUrl);
    }

    [Theory]
    [InlineData("invalid-url")]
    [InlineData("ftp://invalid.com")]
    [InlineData("not-a-url")]
    [InlineData("www.missing-protocol.com")]
    public void Validate_InvalidWebsiteUrl_ShouldHaveValidationError(string websiteUrl)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            websiteUrl
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl)
            .WithErrorMessage("Invalid website URL format");
    }

    [Fact]
    public void Validate_EmptyWebsiteUrl_ShouldNotHaveValidationError()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            ""
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.WebsiteUrl);
    }

    [Theory]
    [InlineData("https://validlogo.com/logo.png")]
    [InlineData("http://validlogo.com/logo.jpg")]
    [InlineData("https://cdn.example.com/images/logo.gif")]
    public void Validate_ValidLogoUrl_ShouldNotHaveValidationError(string logoUrl)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            "https://validclub.com",
            logoUrl
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LogoUrl);
    }

    [Theory]
    [InlineData("invalid-logo-url")]
    [InlineData("ftp://invalid.com/logo.png")]
    [InlineData("not-a-url")]
    public void Validate_InvalidLogoUrl_ShouldHaveValidationError(string logoUrl)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            "https://validclub.com",
            logoUrl
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LogoUrl)
            .WithErrorMessage("Invalid logo URL format");
    }

    [Fact]
    public void Validate_EmptyLogoUrl_ShouldNotHaveValidationError()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            "https://validclub.com",
            ""
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LogoUrl);
    }

    [Theory]
    [InlineData("valid@email.com")]
    [InlineData("user.name@domain.co.uk")]
    [InlineData("test+tag@example.org")]
    [InlineData("user123@test-domain.com")]
    public void Validate_ValidContactEmail_ShouldNotHaveValidationError(string contactEmail)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            "https://validclub.com",
            "https://validclub.com/logo.png",
            contactEmail
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user.domain.com")]
    public void Validate_InvalidContactEmail_ShouldHaveValidationError(string contactEmail)
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            "https://validclub.com",
            "https://validclub.com/logo.png",
            contactEmail
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmptyContactEmail_ShouldNotHaveValidationError()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "Valid Club",
            null,
            null,
            null,
            "https://validclub.com",
            "https://validclub.com/logo.png",
            ""
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Fact]
    public void Validate_MultipleValidationErrors_ShouldHaveAllErrors()
    {
        // Arrange
        CreateClubCommand command = new CreateClubCommand(
            "", // Invalid name
            new string('B', 51), // City too long
            new string('C', 51), // Country too long
            DateTime.UtcNow.AddDays(1), // Future date
            "invalid-url", // Invalid website URL
            "invalid-logo-url", // Invalid logo URL
            "invalid-email" // Invalid email
        );

        // Act
        TestValidationResult<CreateClubCommand> result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.City);
        result.ShouldHaveValidationErrorFor(x => x.Country);
        result.ShouldHaveValidationErrorFor(x => x.FoundingDate);
        result.ShouldHaveValidationErrorFor(x => x.WebsiteUrl);
        result.ShouldHaveValidationErrorFor(x => x.LogoUrl);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }
} 
