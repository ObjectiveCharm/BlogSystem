using Blog.Entities.Types;

namespace Blog.DTOs;

public record CreateArticleRequest(string Title, string Content, Guid AuthorId, List<Guid> TagIds);
public record UpdateArticleRequest(string Title, string Content, List<Guid> TagIds);
public record ArticleResponse(Guid Id, string Title, string Content, Guid AuthorId, DateTime CreatedAt, DateTime UpdatedAt, ArticleStatus Status, List<Guid> TagIds); // Simplified for now
