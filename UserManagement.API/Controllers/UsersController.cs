using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;
using UserManagement.Application.Wrappers;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto dto)
    {
        _logger.LogInformation(
            "📥 Received user registration request. Phone: {Phone}, Name: {Name}",
            dto.Phone, dto.Name);

        try
        {
            var userId = await _userService.RegisterUserAsync(dto);

            _logger.LogInformation(
                "📤 User registration request completed successfully. UserId: {UserId}, Phone: {Phone}",
                userId, dto.Phone);

            return Ok(new ApiResponse<int>(userId, "User created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ User registration request failed. Phone: {Phone}, Error: {ErrorMessage}",
                dto.Phone, ex.Message);
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("📥 Received request to retrieve all users");

        try
        {
            var users = await _userService.GetAllAsync();
            var userList = users?.ToList() ?? new List<UserResponseDto>();

            _logger.LogInformation(
                "📤 Retrieved all users successfully. Count: {UserCount}",
                userList.Count);

            return Ok(userList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to retrieve all users");
            throw;
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("📥 Received request to retrieve user by ID. UserId: {UserId}", id);

        try
        {
            var user = await _userService.GetByIdAsync(id);

            _logger.LogInformation(
                "📤 Retrieved user successfully. UserId: {UserId}, Name: {Name}",
                id, user?.Name ?? "Unknown");

            return Ok(new ApiResponse<UserResponseDto>(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to retrieve user by ID. UserId: {UserId}",
                id);
            throw;
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUserDto dto)
    {
        _logger.LogInformation(
            "📥 Received request to update user. UserId: {UserId}, NewName: {Name}",
            id, dto.Name);

        try
        {
            await _userService.UpdateAsync(id, dto);

            _logger.LogInformation(
                "📤 User updated successfully. UserId: {UserId}",
                id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to update user. UserId: {UserId}",
                id);
            throw;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("📥 Received request to delete user. UserId: {UserId}", id);

        try
        {
            await _userService.DeleteAsync(id);

            _logger.LogInformation(
                "📤 User deleted successfully. UserId: {UserId}",
                id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to delete user. UserId: {UserId}",
                id);
            throw;
        }
    }
}