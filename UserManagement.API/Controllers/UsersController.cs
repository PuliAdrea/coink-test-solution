using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;

namespace UserManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var userId = await _userService.RegisterUserAsync(dto);
            return Ok(new { Message = "User registered successfully", UserId = userId });
        }
        catch (ArgumentException ex)
        {
            // Return 400 for validation errors
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            // Return 500 for generic server errors
            return StatusCode(500, new { Error = "Internal Server Error", Details = ex.Message });
        }
    }
}