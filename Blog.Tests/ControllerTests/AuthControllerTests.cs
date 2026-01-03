using Blog.Controllers;
using Blog.DTOs;
using Blog.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Security.Claims;

namespace Blog.Tests.ControllerTests;

[TestFixture]
public class AuthControllerTests
{
    private class MockAuthService : IAuthService
    {
        public bool ShouldLoginSucceed = true;
        public bool ShouldRefreshSucceed = true;
        public bool ShouldChangePasswordSucceed = true;

        public Task<(string Token, string RefreshToken)?> LoginAsync(string username, string password) 
        {
            if (ShouldLoginSucceed) return Task.FromResult<(string, string)?>(("token", "refresh"));
            return Task.FromResult<(string, string)?>(null);
        }

        public Task<string?> RefreshTokenAsync(string refreshToken)
        {
            if (ShouldRefreshSucceed) return Task.FromResult<string?>("newtoken");
            return Task.FromResult<string?>(null);
        }

        public Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            return Task.FromResult(ShouldChangePasswordSucceed);
        }

        public string HashPassword(string password) => "hash";
        public bool VerifyPassword(string password, string hash) => true;
    }

    [Test]
    public async Task Login_ShouldReturnOk_WhenSuccess()
    {
        var authService = new MockAuthService();
        var controller = new AuthController(authService);
        var request = new LoginRequest("user", "pass");

        var result = await controller.Login(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenFailed()
    {
        var authService = new MockAuthService { ShouldLoginSucceed = false };
        var controller = new AuthController(authService);
        var request = new LoginRequest("user", "pass");

        var result = await controller.Login(request);

        Assert.That(result, Is.TypeOf<UnauthorizedObjectResult>());
    }

    [Test]
    public async Task RefreshToken_ShouldReturnOk_WhenSuccess()
    {
        var authService = new MockAuthService();
        var controller = new AuthController(authService);
        var request = new RefreshTokenRequest("refresh");

        var result = await controller.RefreshToken(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task ChangePassword_ShouldReturnOk_WhenSuccess()
    {
        var authService = new MockAuthService();
        var controller = new AuthController(authService);
        
        // Mock User Context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] 
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        }));
        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        var request = new ChangePasswordRequest("old", "new");

        var result = await controller.ChangePassword(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
    }
}
