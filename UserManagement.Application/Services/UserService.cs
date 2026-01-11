using System.Text.RegularExpressions;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Exceptions;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<int> RegisterUserAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name is required.");

        if (string.IsNullOrWhiteSpace(dto.Address))
            throw new ArgumentException("Address is required.");

        if (dto.MunicipalityId <= 0)
            throw new ArgumentException("Valid Municipality is required.");

        var phoneRegex = new Regex(@"^\+?\d{7,15}$");
        if (!phoneRegex.IsMatch(dto.Phone))
        {
            throw new ArgumentException("Invalid phone number format.");
        }

        var user = new User
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Address = dto.Address,
            MunicipalityId = dto.MunicipalityId
        };

        return await _userRepository.RegisterUserAsync(user);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();
        return users.Select(u => new UserResponseDto(
            u.id, u.name, u.phone, u.address, u.municipalityname, u.departmentname, u.countryname));
    }

    public async Task<UserResponseDto> GetByIdAsync(int id)
    {
        var u = await _userRepository.GetUserByIdAsync(id);
        if (u == null)
            throw new NotFoundException("User", id);

        return new UserResponseDto(u.Id, u.Name, u.Phone, u.Address, "", "", "");
    }

    public async Task UpdateAsync(int id, CreateUserDto dto)
    {
        var existing = await _userRepository.GetUserByIdAsync(id);
        if (existing == null)
            throw new NotFoundException("User", id);

        var user = new User { Id = id, Name = dto.Name, Phone = dto.Phone, Address = dto.Address, MunicipalityId = dto.MunicipalityId };
        await _userRepository.UpdateUserAsync(user);
    }

    public async Task DeleteAsync(int id)
    {
        await _userRepository.DeleteUserAsync(id);
    }
}