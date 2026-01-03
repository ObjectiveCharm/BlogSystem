namespace Blog.DTOs;

public record CreateTagRequest(string Name);
public record UpdateTagRequest(string Name);
public record TagResponse(Guid Id, string Name, DateTime CreatedAt);
