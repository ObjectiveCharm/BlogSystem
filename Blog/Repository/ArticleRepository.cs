using Blog.Data;
using Blog.Entities;
using Blog.Entities.Types;
using Blog.Repository.Types;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repository;

public partial class ArticleRepository(BlogDbContext context)
{
    
}

partial class ArticleRepository
{
    #region Article By Id
    /// <summary>
    /// Finds an article by its identifier.
    /// </summary>
    /// <param name="id">The article identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The article if found; otherwise, null.</returns>
    public async Task<Article?> FindByIdAsync(Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await context.Articles.FindAsync(new object[] { id }, cancellationToken);
    }

    // Save article to database and return old version if exists
    /// <summary>
    /// Saves an article to the database. If it exists, updates it; otherwise, adds a new article.
    /// </summary>
    /// <param name="article">The article entity to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved article entity.</returns>
    public async Task<Article?> SaveAsync(Article article,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.Articles.FindAsync(new object[] { article.Id }, cancellationToken);
        if (existing == null)
        {
            await context.Articles.AddAsync(article, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return article;
        }

        context.Entry(existing).CurrentValues.SetValues(article);
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }
    #endregion
}

partial class ArticleRepository
{
    #region Change Article Status
    public async Task<ArticleStatus?> PublishAsync(Guid id,
        CancellationToken cancellationToken = default) => await ChangeStatusAsync(id, ArticleStatus.PUBLISHED, cancellationToken);
    
    public async Task<ArticleStatus?> HiddenAsync(Guid id, 
        CancellationToken cancellationToken = default) => await ChangeStatusAsync(id, ArticleStatus.HIDDEN, cancellationToken);
    
    // Change article status and return old status if article exists
    /// <summary>
    /// Changes the status of an article.
    /// </summary>
    /// <param name="id">The article identifier.</param>
    /// <param name="status">The new status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The old status if the article exists; otherwise, null.</returns>
    private async Task<ArticleStatus?> ChangeStatusAsync(Guid id, ArticleStatus status, CancellationToken cancellationToken = default)
    {
        var article = await context.Articles.FindAsync(new object[] { id }, cancellationToken);
        if (article == null) return null;

        var oldStatus = article.Status;
        article.Status = status;
        await context.SaveChangesAsync(cancellationToken);
        return oldStatus;
    }
    #endregion
}

partial class ArticleRepository
{
    #region Get Articles By Author Id with keyset pagination
    /// <summary>
    /// Lists articles by author identifier with keyset pagination.
    /// </summary>
    /// <param name="authorId">The author identifier.</param>
    /// <param name="bulkSize">Number of articles to retrieve.</param>
    /// <param name="lastSeekCursor">Cursor for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of articles and a flag indicating if there are more.</returns>
    public async Task<(List<Article> articles, bool hasMore)> ListByAuthorIdAsync(
        Guid authorId,
        uint bulkSize = 10,
        LastSeekCursor? lastSeekCursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Articles.Where(a => a.AuthorId == authorId);

        if (lastSeekCursor != null)
        {
            query = query.Where(a => a.CreatedAt < lastSeekCursor.LastCreatedAt || 
                                     (a.CreatedAt == lastSeekCursor.LastCreatedAt && a.Id < lastSeekCursor.LastId));
        }

        query = query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id);

        var articles = await query.Take((int)bulkSize + 1).ToListAsync(cancellationToken);

        var hasMore = articles.Count > bulkSize;
        if (hasMore)
        {
            articles.RemoveAt(articles.Count - 1);
        }

        return (articles, hasMore);
    }
    
    #endregion
}

partial class ArticleRepository
{
    #region List Get Articles By Tag with keyset pagination
    /// <summary>
    /// Lists articles by tag identifier with keyset pagination.
    /// </summary>
    /// <param name="tagId">The tag identifier.</param>
    /// <param name="bulkSize">Number of articles to retrieve.</param>
    /// <param name="lastSeekCursor">Cursor for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of articles and a flag indicating if there are more.</returns>
    public async Task<(List<Article> articles, bool hasMore)> ListByTagAsync(
        Guid tagId,
        uint bulkSize = 10,
        LastSeekCursor? lastSeekCursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Articles.Where(a => a.Tags.Any(t => t.Id == tagId));

        if (lastSeekCursor != null)
        {
            query = query.Where(a => a.CreatedAt < lastSeekCursor.LastCreatedAt || 
                                     (a.CreatedAt == lastSeekCursor.LastCreatedAt && a.Id < lastSeekCursor.LastId));
        }

        query = query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id);

        var articles = await query.Take((int)bulkSize + 1).ToListAsync(cancellationToken);

        var hasMore = articles.Count > bulkSize;
        if (hasMore)
        {
            articles.RemoveAt(articles.Count - 1);
        }

        return (articles, hasMore);
        
    }
    #endregion
}

partial class ArticleRepository
{
    #region List All Articles with keyset pagination
    /// <summary>
    /// Lists all articles with keyset pagination.
    /// </summary>
    /// <param name="bulkSize">Number of articles to retrieve.</param>
    /// <param name="lastSeekCursor">Cursor for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing the list of articles and a flag indicating if there are more.</returns>
    public async Task<(List<Article> articles, bool hasMore)> ListAllArticleAsync(
        uint bulkSize = 10,
        LastSeekCursor? lastSeekCursor = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Articles.AsQueryable();

        if (lastSeekCursor != null)
        {
            query = query.Where(a => a.CreatedAt < lastSeekCursor.LastCreatedAt || 
                                     (a.CreatedAt == lastSeekCursor.LastCreatedAt && a.Id < lastSeekCursor.LastId));
        }

        query = query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id);

        var articles = await query.Take((int)bulkSize + 1).ToListAsync(cancellationToken);

        var hasMore = articles.Count > bulkSize;
        if (hasMore)
        {
            articles.RemoveAt(articles.Count - 1);
        }

        return (articles, hasMore);
    }
    #endregion
}