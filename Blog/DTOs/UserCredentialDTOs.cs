namespace Blog.DTOs;

public record CreateUserCredentialRequest(string Email, string Password);
public record UpdateUserCredentialRequest(string Email, string Password); // Usually just password or email
public record UserCredentialResponse(Guid UserId, string Email, DateTime CreatedAt, DateTime LastChangedAt);
