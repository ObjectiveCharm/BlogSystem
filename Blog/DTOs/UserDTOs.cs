namespace Blog.DTOs;

public record CreateUserRequest(string Username);
public record UpdateUserRequest(string Username);
public record UserResponse(Guid Id, string Username);
