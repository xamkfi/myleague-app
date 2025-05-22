//using Application.UseCases.Clubs;
//using Domain.Entities.Common;
//using Domain.Repositories.Common;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Xunit;

//namespace Application.Tests.UseCases.Clubs;

//public class CreateClubUseCaseTests
//{
//    private readonly Mock<IClubRepository> _mockClubRepository;
//    private readonly Mock<ILogger<CreateClubUseCase>> _mockLogger;
//    private readonly CreateClubUseCase _useCase;

//    public CreateClubUseCaseTests()
//    {
//        _mockClubRepository = new Mock<IClubRepository>();
//        _mockLogger = new Mock<ILogger<CreateClubUseCase>>();
//        _useCase = new CreateClubUseCase(_mockClubRepository.Object, _mockLogger.Object);
//    }

//    [Fact]
//    public async Task ExecuteAsync_WithValidData_ShouldCreateClub()
//    {
//        // Arrange
//        var name = "Test Club";
//        var city = "Test City";
//        var country = "Test Country";
        
//        _mockClubRepository.Setup(r => r.ExistsByNameAsync(name))
//            .ReturnsAsync(false);
        
//        _mockClubRepository.Setup(r => r.AddAsync(It.IsAny<Club>()))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _useCase.ExecuteAsync(name, city, country);

//        // Assert
//        Assert.NotNull(result);
//        Assert.Equal(name, result.Name);
//        Assert.Equal(city, result.City);
//        Assert.Equal(country, result.Country);
        
//        _mockClubRepository.Verify(r => r.ExistsByNameAsync(name), Times.Once);
//        _mockClubRepository.Verify(r => r.AddAsync(It.IsAny<Club>()), Times.Once);
//    }

//    [Fact]
//    public async Task ExecuteAsync_WithExistingName_ShouldThrowInvalidOperationException()
//    {
//        // Arrange
//        var name = "Existing Club";
//        var city = "Test City";
//        var country = "Test Country";
        
//        _mockClubRepository.Setup(r => r.ExistsByNameAsync(name))
//            .ReturnsAsync(true);

//        // Act & Assert
//        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
//            _useCase.ExecuteAsync(name, city, country));
        
//        Assert.Contains(name, exception.Message);
//        _mockClubRepository.Verify(r => r.ExistsByNameAsync(name), Times.Once);
//        _mockClubRepository.Verify(r => r.AddAsync(It.IsAny<Club>()), Times.Never);
//    }

//    [Theory]
//    [InlineData(null, "City", "Country")]
//    [InlineData("", "City", "Country")]
//    [InlineData("Club", null, "Country")]
//    [InlineData("Club", "", "Country")]
//    [InlineData("Club", "City", null)]
//    [InlineData("Club", "City", "")]
//    public async Task ExecuteAsync_WithInvalidData_ShouldThrowArgumentException(string name, string city, string country)
//    {
//        // Act & Assert
//        await Assert.ThrowsAsync<ArgumentNullException>(() => 
//            _useCase.ExecuteAsync(name, city, country));
        
//        _mockClubRepository.Verify(r => r.ExistsByNameAsync(It.IsAny<string>()), Times.Never);
//        _mockClubRepository.Verify(r => r.AddAsync(It.IsAny<Club>()), Times.Never);
//    }
//} 
