using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<int> RegisterUserAsync(CreateUserDto dto)
    {
        _logger.LogInformation(
            "Starting user registration process for phone: {Phone}",
            dto.Phone);

        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning(
                    "User registration failed: Name is required. Phone: {Phone}",
                    dto.Phone);
                throw new ArgumentException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Address))
            {
                _logger.LogWarning(
                    "User registration failed: Address is required. Phone: {Phone}",
                    dto.Phone);
                throw new ArgumentException("Address is required.");
            }

            if (dto.MunicipalityId <= 0)
            {
                _logger.LogWarning(
                    "User registration failed: Invalid Municipality ID {MunicipalityId}. Phone: {Phone}",
                    dto.MunicipalityId, dto.Phone);
                throw new ArgumentException("Valid Municipality is required.");
            }

            var phoneRegex = new Regex(@"^\+?\d{7,15}$");
            if (!phoneRegex.IsMatch(dto.Phone))
            {
                _logger.LogWarning(
                    "User registration failed: Invalid phone format. Phone: {Phone}",
                    dto.Phone);
                throw new ArgumentException("Invalid phone number format.");
            }

            _logger.LogDebug(
                "All validations passed for user registration. Name: {Name}, Phone: {Phone}, MunicipalityId: {MunicipalityId}",
                dto.Name, dto.Phone, dto.MunicipalityId);

            var user = new User
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                MunicipalityId = dto.MunicipalityId
            };

            var userId = await _userRepository.RegisterUserAsync(user);

            _logger.LogInformation(
                "✅ User successfully registered. UserId: {UserId}, Name: {Name}, Phone: {Phone}, Address: {Address}, MunicipalityId: {MunicipalityId}",
                userId, dto.Name, dto.Phone, dto.Address, dto.MunicipalityId);

            return userId;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex,
                "Validation error during user registration. Phone: {Phone}, Error: {ErrorMessage}",
                dto.Phone, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during user registration. Phone: {Phone}",
                dto.Phone);
            throw;
        }
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all users from database");

        try
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userList = users.ToList();

            _logger.LogInformation(
                "Successfully retrieved {UserCount} users from database",
                userList.Count);

            return userList.Select(u => new UserResponseDto(
                u.id, u.name, u.phone, u.address,
                u.municipalityname, u.departmentname, u.countryname));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users from database");
            throw;
        }
    }

    public async Task<UserResponseDto> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving user by ID: {UserId}", id);

        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("User not found. UserId: {UserId}", id);
                throw new NotFoundException("User", id);
            }

            _logger.LogInformation(
                "Successfully retrieved user. UserId: {UserId}, Name: {Name}",
                user.Id, user.Name);

            return new UserResponseDto(
                user.Id, user.Name, user.Phone, user.Address, "", "", "");
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving user by ID. UserId: {UserId}",
                id);
            throw;
        }
    }

    public async Task UpdateAsync(int id, CreateUserDto dto)
    {
        _logger.LogInformation(
            "Starting user update process. UserId: {UserId}, NewName: {Name}",
            id, dto.Name);

        try
        {
            var existing = await _userRepository.GetUserByIdAsync(id);

            if (existing == null)
            {
                _logger.LogWarning(
                    "Update failed: User not found. UserId: {UserId}",
                    id);
                throw new NotFoundException("User", id);
            }

            _logger.LogDebug(
                "Existing user found. UserId: {UserId}, CurrentName: {CurrentName}, NewName: {NewName}",
                id, existing.Name, dto.Name);

            var user = new User
            {
                Id = id,
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                MunicipalityId = dto.MunicipalityId
            };

            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation(
                "✅ User successfully updated. UserId: {UserId}, Name: {Name}, Phone: {Phone}",
                id, dto.Name, dto.Phone);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating user. UserId: {UserId}",
                id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Starting user deletion process. UserId: {UserId}", id);

        try
        {
            await _userRepository.DeleteUserAsync(id);

            _logger.LogInformation(
                "✅ User successfully deleted. UserId: {UserId}",
                id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting user. UserId: {UserId}",
                id);
            throw;
        }
    }
}