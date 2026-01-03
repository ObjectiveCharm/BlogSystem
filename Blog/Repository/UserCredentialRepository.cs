using Blog.Data;
using Blog.Entities;

namespace Blog.Repository;

public partial class UserCredentialRepository(BlogDbContext context)
{
    
}
partial class UserCredentialRepository
{
    #region Find by Id

    /// <summary>
    /// Finds user credentials by user identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user credential if found; otherwise, null.</returns>
    public async Task<UserCredential?> FindByIdAsync(Guid id, 
        CancellationToken cancellationToken = default)
    {
        return await context.UserCredentials.FindAsync(id , cancellationToken);
    }
    
    // Save user credential to database and return old version if exists
    /// <summary>
    /// Saves user credentials to the database. If they exist, updates them; otherwise, adds new credentials.
    /// </summary>
    /// <param name="userCredential">The user credential entity to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The saved user credential entity.</returns>
    public async Task<UserCredential?> SaveAsync(UserCredential userCredential,
        CancellationToken cancellationToken = default)
    {
        var existing = await context.UserCredentials.FindAsync(userCredential.UserId, cancellationToken);
        if (existing == null)
        {
            await context.UserCredentials.AddAsync(userCredential, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return userCredential;
        }

        context.Entry(existing).CurrentValues.SetValues(userCredential);
        await context.SaveChangesAsync(cancellationToken);
        return existing;
    }
    #endregion
}