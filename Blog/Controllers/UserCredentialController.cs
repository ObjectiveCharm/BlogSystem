using Blog.DTOs;
using Blog.Entities;
using Blog.Repository;
using Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.Controllers;

[ApiController]
[Route("")]
public class UserCredentialController(
    UserCredentialRepository userCredentialRepository,
    IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Get user credential by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User credential details</returns>
    [HttpGet("usercredential/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Get user credential", Description = "Get user credential by User ID")]
    public async Task<IActionResult> GetUserCredential(Guid id)
    {
        var credential = await userCredentialRepository.FindByIdAsync(id);
        if (credential == null) return NotFound();
        return Ok(credential);
    }

    /// <summary>
    /// Create user credential
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Credential creation request</param>
    /// <returns>Created credential</returns>
    [HttpPost("usercredential/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Create user credential", Description = "Create a new user credential")]
    public async Task<IActionResult> CreateUserCredential(Guid id, [FromBody] CreateUserCredentialRequest request)
    {
        // Check if exists? SaveAsync handles it.
        var credential = new UserCredential
        {
            UserId = id,
            Email = request.Email,
            PasswordHash = authService.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            LastChangedAt = DateTime.UtcNow,
            EmailConfirmed = false // Default
        };
        
        await userCredentialRepository.SaveAsync(credential);
        return Ok(credential);
    }

    /// <summary>
    /// Update user credential
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated credential</returns>
    [HttpPut("usercredential/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Update user credential", Description = "Update an existing user credential")]
    public async Task<IActionResult> UpdateUserCredential(Guid id, [FromBody] UpdateUserCredentialRequest request)
    {
        var credential = await userCredentialRepository.FindByIdAsync(id);
        if (credential == null) return NotFound();

        credential.Email = request.Email;
        // Should we update password here? DTO has Password.
        // Usually password update is separate or requires old password. 
        // But this is an admin-like or direct update endpoint.
        if (!string.IsNullOrEmpty(request.Password))
        {
             credential.PasswordHash = authService.HashPassword(request.Password);
             credential.LastChangedAt = DateTime.UtcNow;
        }
        
        await userCredentialRepository.SaveAsync(credential);
        return Ok(credential);
    }
}
