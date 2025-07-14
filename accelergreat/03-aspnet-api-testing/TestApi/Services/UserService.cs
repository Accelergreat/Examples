using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApi.Models;

namespace TestApi.Services;

public class UserService : IUserService
{
    private readonly List<User> _users;
    private int _nextId = 1;

    public UserService()
    {
        _users = new List<User>
        {
            new User { Id = _nextId++, Name = "John Doe", Email = "john@example.com", CreatedAt = DateTime.UtcNow },
            new User { Id = _nextId++, Name = "Jane Smith", Email = "jane@example.com", CreatedAt = DateTime.UtcNow },
            new User { Id = _nextId++, Name = "Bob Johnson", Email = "bob@example.com", CreatedAt = DateTime.UtcNow }
        };
        _nextId = 4; // Set next ID after seeded data
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

    public Task<User> CreateUserAsync(User user)
    {
        user.Id = _nextId++;
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User?> UpdateUserAsync(int id, User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        if (existingUser == null)
            return Task.FromResult<User?>(null);

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        return Task.FromResult<User?>(existingUser);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id && u.IsActive);
        if (user == null)
            return Task.FromResult(false);

        user.IsActive = false; // Soft delete
        return Task.FromResult(true);
    }
} 