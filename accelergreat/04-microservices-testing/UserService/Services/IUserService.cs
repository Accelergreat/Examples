using UserService.Models;

namespace UserService.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User?> UpdateUserAsync(int id, CreateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ValidateUserAsync(int id);
} 