using Moq;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions; 
using UserManagement.Domain.Interfaces;


namespace UserManagement.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _userService = new UserService(_mockRepository.Object);
    }

    [Fact]
    public async Task RegisterUser_WithValidData_ShouldReturnId()
    {
        // ARRANGE
        var userDto = new CreateUserDto
        {
            Name = "Test User",
            Phone = "+573001234567",
            Address = "Test Address",
            MunicipalityId = 1
        };

        _mockRepository.Setup(repo => repo.RegisterUserAsync(It.IsAny<User>()))
                       .ReturnsAsync(10);
        // ACT 
        var resultId = await _userService.RegisterUserAsync(userDto);

        // ASSERT 
        Assert.Equal(10, resultId); 
        _mockRepository.Verify(repo => repo.RegisterUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WithInvalidPhone_ShouldThrowException()
    {
        // ARRANGE
        var invalidDto = new CreateUserDto
        {
            Name = "Test User",
            Phone = "123", 
            Address = "Address",
            MunicipalityId = 1
        };

        // ACT & ASSERT
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _userService.RegisterUserAsync(invalidDto));

        Assert.Equal("Invalid phone number format.", exception.Message);
        _mockRepository.Verify(repo => repo.RegisterUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // ARRANGE
        int userId = 99;
        _mockRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                       .ReturnsAsync((User?)null);

        // ACT & ASSERT
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _userService.GetByIdAsync(userId));
    }
}