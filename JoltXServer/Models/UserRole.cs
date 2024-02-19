namespace JoltXServer.Models;

public class UserRole(string email, string role)
{
    public string Email { get; set; } = email;
    public string Role { get; set; } = role;
}

