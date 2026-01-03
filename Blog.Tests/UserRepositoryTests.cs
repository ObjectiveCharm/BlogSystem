using Blog.Data;
using Blog.Entities;
using Blog.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;

namespace Blog.Tests;

[TestFixture]
public class UserRepositoryTests
{
    private BlogDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task SaveAsync_ShouldAddUser_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new UserRepository(context);
        var user = new User { Id = Guid.NewGuid(), Username = "testuser" };

        // Act
        var result = await repository.SaveAsync(user);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(user.Id));
        var savedUser = await context.Users.FindAsync(user.Id);
        Assert.That(savedUser, Is.Not.Null);
    }

    [Test]
    public async Task SaveAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var initialUser = new User { Id = userId, Username = "initial" };
        context.Users.Add(initialUser);
        await context.SaveChangesAsync();

        var updatedUser = new User { Id = userId, Username = "updated" };

        // Act
        var result = await repository.SaveAsync(updatedUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Username, Is.EqualTo("updated"));
        var inDbUser = await context.Users.FindAsync(userId);
        Assert.That(inDbUser, Is.Not.Null);
        Assert.That(inDbUser!.Username, Is.EqualTo("updated"));
    }

    [Test]
    public async Task FindById_ShouldReturnUser_WhenExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "findme" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.FindById(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(userId));
    }

    [Test]
    public async Task ListArticlesByUserIdAsync_ShouldReturnArticles()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new UserRepository(context);
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "author" };
        context.Users.Add(user);

        context.Articles.AddRange(
            new Article { Id = Guid.NewGuid(), Title = "A1", AuthorId = userId },
            new Article { Id = Guid.NewGuid(), Title = "A2", AuthorId = userId },
            new Article { Id = Guid.NewGuid(), Title = "Other", AuthorId = Guid.NewGuid() }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await repository.ListArticlesByUserIdAsync(userId);

        // Assert
        Assert.That(results.Count, Is.EqualTo(2));
        foreach (var a in results)
        {
            Assert.That(a.AuthorId, Is.EqualTo(userId));
        }
    }
}