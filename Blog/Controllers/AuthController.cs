using Blog.DTOs;
using Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.Controllers;

[ApiController]
[Route("")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Login
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>JWT Token</returns>
    [HttpPost("login")]
    [SwaggerOperation(Summary = "Login", Description = "Login with username and password")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request.Username, request.Password);
        if (result == null) return Unauthorized("Invalid username or password");
        
        return Ok(new { Token = result.Value.Token, RefreshToken = result.Value.RefreshToken });
    }

    /// <summary>
    /// Refresh Token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT Token</returns>
    [HttpPost("refresh-token")]
    [SwaggerOperation(Summary = "Refresh Token", Description = "Get a new access token using a refresh token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var token = await authService.RefreshTokenAsync(request.RefreshToken);
        if (token == null) return Unauthorized("Invalid refresh token");
        
        return Ok(new { Token = token });
    }

    /// <summary>
    /// Change Password
    /// </summary>
    /// <param name="request">Change password request</param>
    /// <returns>Result</returns>
    [HttpPost("change-password")]
    [Authorize]
    [SwaggerOperation(Summary = "Change Password", Description = "Change current user's password and invalidate other sessions")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var success = await authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
        if (!success) return BadRequest("Failed to change password. Old password might be incorrect.");
        
        return Ok("Password changed successfully.");
    }
}
