namespace UserService.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public UserPreferences Preferences { get; set; } = new();
}

public class UserPreferences
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public string TimeZone { get; set; } = "UTC";
    public string Language { get; set; } = "en";
} 