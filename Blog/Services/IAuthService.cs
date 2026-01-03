using Blog.DTOs;

namespace Blog.Services;

public interface IAuthService
{
    Task<(string Token, string RefreshToken)?> LoginAsync(string username, string password);
    Task<string?> RefreshTokenAsync(string refreshToken); 
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}