using Blog.Data;
using Blog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Repository;

public partial class UserRepository(BlogDbContext context)
{
    
}

partial class UserRepository
{
    #region Find by Id

    /// <summary>
    /// Finds a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    public async Task<User?> FindById(Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await context.Users.FindAsync(new object[] { id }, cancellationToken);
    }
    
    // Save user to database and return old version if exists
    /// <summary>
    /// Saves a user to the database. If the user exists, updates it; otherwise, adds a new user.
    /// </summary>
    /// <param name="user">The user entity to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved user entity.</returns>
    public async Task<User?> SaveAsync(User user,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.Users.FindAsync(new object[] { user.Id }, cancellationToken);
        if (existing == null)
        {
            await context.Users.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return user;
        }

        context.Entry(existing).CurrentValues.SetValues(user);
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }
    #endregion
}

partial class UserRepository
{
    #region Find by Username
    /// <summary>
    /// Finds a user by their username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    public async Task<User?> FindByUsernameAsync(string username, 
        CancellationToken cancellationToken = default)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }
    #endregion
}

partial class UserRepository
{
    #region List all user articles by user id

    /// <summary>
    /// Lists all articles authored by a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of articles.</returns>
    public async Task<List<Article>> ListArticlesByUserIdAsync(
        Guid userId,
        
        CancellationToken cancellationToken = default)
    {
        return await context.Articles
            .Where(a => a.AuthorId == userId)
            .ToListAsync(cancellationToken);
    }
    #endregion
}