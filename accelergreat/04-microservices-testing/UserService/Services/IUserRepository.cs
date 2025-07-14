using UserService.Models;

namespace UserService.Services;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<User?> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(int id);
} 