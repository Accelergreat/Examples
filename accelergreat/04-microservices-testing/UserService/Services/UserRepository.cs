using UserService.Models;

namespace UserService.Services;

public class UserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public UserRepository()
    {
        // Seed some initial data
        _users.AddRange(new[]
        {
            new User
            {
                Id = _nextId++,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "+1234567890",
                CreatedAt = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    EmailNotifications = true,
                    SmsNotifications = false,
                    TimeZone = "UTC",
                    Language = "en"
                }
            },
            new User
            {
                Id = _nextId++,
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                Phone = "+1234567891",
                CreatedAt = DateTime.UtcNow,
                Preferences = new UserPreferences
                {
                    EmailNotifications = true,
                    SmsNotifications = true,
                    TimeZone = "EST",
                    Language = "en"
                }
            }
        });
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.Where(u => u.IsActive).AsEnumerable());
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        var user = _users.FirstOrDefault(u => u.Email == email && u.IsActive);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        user.Id = _nextId++;
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> UpdateUserAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id && u.IsActive);
        if (existing == null)
            return Task.FromResult<User?>(null);

        existing.Name = user.Name;
        existing.Email = user.Email;
        existing.Phone = user.Phone;
        existing.Preferences = user.Preferences;
        
        return Task.FromResult<User?>(existing);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        if (user == null)
            return Task.FromResult(false);

        user.IsActive = false;
        return Task.FromResult(true);
    }
} 