using UserService.Models;

namespace UserService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userRepository.GetUserByIdAsync(id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetUserByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Validate email uniqueness
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("User with this email already exists");

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Preferences = request.Preferences ?? new UserPreferences()
        };

        return await _userRepository.CreateUserAsync(user);
    }

    public async Task<User?> UpdateUserAsync(int id, CreateUserRequest request)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(id);
        if (existingUser == null)
            return null;

        // Check if email is being changed and if it's already taken
        if (existingUser.Email != request.Email)
        {
            var userWithEmail = await _userRepository.GetUserByEmailAsync(request.Email);
            if (userWithEmail != null && userWithEmail.Id != id)
                throw new InvalidOperationException("Email already in use by another user");
        }

        existingUser.Name = request.Name;
        existingUser.Email = request.Email;
        existingUser.Phone = request.Phone;
        existingUser.Preferences = request.Preferences ?? existingUser.Preferences;

        return await _userRepository.UpdateUserAsync(existingUser);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _userRepository.DeleteUserAsync(id);
    }

    public async Task<bool> ValidateUserAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        return user != null;
    }
} 