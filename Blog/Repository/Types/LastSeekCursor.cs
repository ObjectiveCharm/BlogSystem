namespace Blog.Repository.Types;

public record LastSeekCursor(DateTime LastCreatedAt, 
    Guid LastId);