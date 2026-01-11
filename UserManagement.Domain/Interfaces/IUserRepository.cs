using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<int> RegisterUserAsync(User user);
        Task<IEnumerable<dynamic>> GetAllUsersAsync(); 
        Task<User?> GetUserByIdAsync(int id);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
    }
}
