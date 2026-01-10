using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Interfaces
{
    public interface IUserRepository
    {
        // Returns the new User ID
        Task<int> RegisterUserAsync(User user);
    }
}
