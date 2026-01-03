using Blog.Data;
using Blog.Entities;
using Blog.Repository.Types;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repository;

public partial class TagRepository(BlogDbContext context)
{
    
}

partial class TagRepository
{
    #region Tag by Id
    /// <summary>
    /// Finds a tag by its identifier.
    /// </summary>
    /// <param name="id">The tag identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tag if found; otherwise, null.</returns>
    public async Task<Tag?> FindByIdAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        return await context.Tags.FindAsync(new object[] { id }, cancellationToken);
    }
    
    // Save tag to database and return old version if exists
    /// <summary>
    /// Saves a tag to the database. If it exists, updates it; otherwise, adds a new tag.
    /// </summary>
    /// <param name="tag">The tag entity to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved tag entity.</returns>
    public async Task<Tag?> SaveAsync(Tag tag,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.Tags.FindAsync(new object[] { tag.Id }, cancellationToken);
        if (existing == null)
        {
            await context.Tags.AddAsync(tag, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return tag;
        }

        context.Entry(existing).CurrentValues.SetValues(tag);
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }
    #endregion
}

partial class TagRepository
{
    #region List All Tags with keyset pagination
    /// <summary>
    /// Lists all tags with keyset pagination.
    /// </summary>
    /// <param name="bulkSize">Number of tags to retrieve.</param>
    /// <param name="lastSeekCursor">Cursor for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of tags and a flag indicating if there are more.</returns>
    public async Task<(List<Tag> tags, bool hasMore)> ListAllTagsAsync(
        uint bulkSize = 10,
        LastSeekCursor? lastSeekCursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Tags.AsQueryable();

        if (lastSeekCursor != null)
        {
            query = query.Where(t => t.CreatedAt < lastSeekCursor.LastCreatedAt || 
                                     (t.CreatedAt == lastSeekCursor.LastCreatedAt && t.Id < lastSeekCursor.LastId));
        }

        query = query.OrderByDescending(t => t.CreatedAt).ThenByDescending(t => t.Id);

        var tags = await query.Take((int)bulkSize + 1).ToListAsync(cancellationToken);

        var hasMore = tags.Count > bulkSize;
        if (hasMore)
        {
            tags.RemoveAt(tags.Count - 1);
        }

        return (tags, hasMore);
    }
    #endregion
}

