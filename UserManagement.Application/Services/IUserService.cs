using UserManagement.Application.DTOs;

namespace UserManagement.Application.Services
{
    public interface IUserService
    {
        Task<int> RegisterUserAsync(CreateUserDto dto);
        Task<IEnumerable<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto?> GetByIdAsync(int id);
        Task UpdateAsync(int id, CreateUserDto dto);
        Task DeleteAsync(int id);
    }
}
