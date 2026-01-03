using Blog.Controllers;
using Blog.Data;
using Blog.DTOs;
using Blog.Entities;
using Blog.Repository;
using Blog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Blog.Tests.ControllerTests;

public class FakeAuthService : IAuthService
{
    public Task<(string Token, string RefreshToken)?> LoginAsync(string username, string password) => Task.FromResult<(string, string)?>(("token", "refresh"));
    public Task<string?> RefreshTokenAsync(string refreshToken) => Task.FromResult<string?>("newtoken");
    public Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword) => Task.FromResult(true);
    public string HashPassword(string password) => "hashed_" + password;
    public bool VerifyPassword(string password, string hash) => hash == "hashed_" + password;
}

[TestFixture]
public class UserCredentialControllerTests
{
    private BlogDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task CreateUserCredential_ShouldReturnOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var repo = new UserCredentialRepository(context);
        var authService = new FakeAuthService();
        var controller = new UserCredentialController(repo, authService);

        var id = Guid.NewGuid();
        var request = new CreateUserCredentialRequest("test@example.com", "password123");
        var result = await controller.CreateUserCredential(id, request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var cred = (UserCredential)okResult.Value;
        Assert.That(cred.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task GetUserCredential_ShouldReturnCredential()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var repo = new UserCredentialRepository(context);
        var authService = new FakeAuthService();
        var controller = new UserCredentialController(repo, authService);

        var id = Guid.NewGuid();
        context.UserCredentials.Add(new UserCredential 
        { 
            UserId = id, 
            Email = "test@example.com", 
            PasswordHash = "hash" 
        });
        await context.SaveChangesAsync();

        var result = await controller.GetUserCredential(id);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var cred = (UserCredential)okResult.Value;
        Assert.That(cred.UserId, Is.EqualTo(id));
    }

    [Test]
    public async Task UpdateUserCredential_ShouldUpdateEmail()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var repo = new UserCredentialRepository(context);
        var authService = new FakeAuthService();
        var controller = new UserCredentialController(repo, authService);

        var id = Guid.NewGuid();
        context.UserCredentials.Add(new UserCredential 
        { 
            UserId = id, 
            Email = "old@example.com", 
            PasswordHash = "hash" 
        });
        await context.SaveChangesAsync();

        var request = new UpdateUserCredentialRequest("new@example.com", "newpass");
        var result = await controller.UpdateUserCredential(id, request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var cred = (UserCredential)okResult.Value;
        Assert.That(cred.Email, Is.EqualTo("new@example.com"));
    }
}
