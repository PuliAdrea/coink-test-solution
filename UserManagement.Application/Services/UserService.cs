using System.Text.RegularExpressions;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Application.Services;

public interface IUserService
{
    Task<int> RegisterUserAsync(CreateUserDto dto);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<int> RegisterUserAsync(CreateUserDto dto)
    {
        // 1. Basic Validations
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.Address))
            throw new ArgumentException("Address is required.");

        if (dto.MunicipalityId <= 0)
            throw new ArgumentException("Valid Municipality is required.");

        // 2. Phone Regex Validation (Simple pattern: 7 to 15 digits, optional +)
        var phoneRegex = new Regex(@"^\+?\d{7,15}$");
        if (!phoneRegex.IsMatch(dto.Phone))
        {
            throw new ArgumentException("Invalid phone number format.");
        }

        // 3. Map DTO to Domain Entity
        var user = new User
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Address = dto.Address,
            MunicipalityId = dto.MunicipalityId
        };

        // 4. Call Repository
        return await _userRepository.RegisterUserAsync(user);
    }
}