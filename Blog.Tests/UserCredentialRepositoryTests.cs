using Blog.Data;
using Blog.Entities;
using Blog.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Blog.Tests;

[TestFixture]
public class UserCredentialRepositoryTests
{
    private BlogDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task SaveAsync_ShouldUpsertCredentials()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UserCredentialRepository(context);
        var userId = Guid.NewGuid();
        var cred = new UserCredential { UserId = userId, Email = "test@test.com", PasswordHash = "hash" };

        // Insert
        await repository.SaveAsync(cred);
        var stored = await context.UserCredentials.FindAsync(userId);
        Assert.That(stored, Is.Not.Null);
        Assert.That(stored!.Email, Is.EqualTo("test@test.com"));

        // Update
        cred.Email = "updated@test.com";
        await repository.SaveAsync(cred);
        stored = await context.UserCredentials.FindAsync(userId);
        Assert.That(stored!.Email, Is.EqualTo("updated@test.com"));
    }
}