using Blog.Controllers;
using Blog.Data;
using Blog.DTOs;
using Blog.Entities;
using Blog.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Blog.Tests.ControllerTests;

[TestFixture]
public class UserControllerTests
{
    private BlogDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task CreateUser_ShouldReturnOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var userRepo = new UserRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new UserController(userRepo, articleRepo);

        var id = Guid.NewGuid();
        var request = new CreateUserRequest("testuser");
        var result = await controller.CreateUser(id, request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var user = (User)okResult.Value;
        Assert.That(user.Username, Is.EqualTo("testuser"));
    }

    [Test]
    public async Task GetUser_ShouldReturnUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var userRepo = new UserRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new UserController(userRepo, articleRepo);

        var id = Guid.NewGuid();
        context.Users.Add(new User { Id = id, Username = "testuser" });
        await context.SaveChangesAsync();

        var result = await controller.GetUser(id);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var user = (User)okResult.Value;
        Assert.That(user.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task UpdateUser_ShouldUpdateUsername()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var userRepo = new UserRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new UserController(userRepo, articleRepo);

        var id = Guid.NewGuid();
        context.Users.Add(new User { Id = id, Username = "oldname" });
        await context.SaveChangesAsync();

        var request = new UpdateUserRequest("newname");
        var result = await controller.UpdateUser(id, request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var user = (User)okResult.Value;
        Assert.That(user.Username, Is.EqualTo("newname"));
    }

    [Test]
    public async Task GetUserArticles_ShouldReturnArticles()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var userRepo = new UserRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new UserController(userRepo, articleRepo);

        var id = Guid.NewGuid();
        context.Articles.Add(new Article 
        { 
            Id = Guid.NewGuid(), 
            Title = "User Article", 
            AuthorId = id, 
            CreatedAt = DateTime.UtcNow 
        });
        await context.SaveChangesAsync();

        var result = await controller.GetUserArticles(id, null, 10);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        
        var value = okResult.Value;
        var dataProp = value.GetType().GetProperty("Data");
        var list = dataProp.GetValue(value) as System.Collections.IEnumerable;
        
        int count = 0;
        foreach (var item in list) count++;
        Assert.That(count, Is.EqualTo(1));
    }
}
