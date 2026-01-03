namespace Blog.DTOs;

public record LoginRequest(string Username, string Password);
public record ChangePasswordRequest(string OldPassword, string NewPassword);
public record RefreshTokenRequest(string RefreshToken);