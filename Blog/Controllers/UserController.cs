using Blog.DTOs;
using Blog.Entities;
using Blog.Repository;
using Blog.Repository.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.Controllers;

[ApiController]
[Route("")]
public class UserController(
    UserRepository userRepository,
    ArticleRepository articleRepository) : ControllerBase
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("user/{id}")]
    [SwaggerOperation(Summary = "Get user", Description = "Get user by ID")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await userRepository.FindById(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    /// <summary>
    /// Create user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">User creation request</param>
    /// <returns>Created user</returns>
    [HttpPost("user/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Create user", Description = "Create a new user with specific ID")]
    public async Task<IActionResult> CreateUser(Guid id, [FromBody] CreateUserRequest request)
    {
        // Check if user exists? Repository.SaveAsync handles "Add if not exists" but usually checks existing first.
        // We can just construct and save.
        var user = new User
        {
            Id = id,
            Username = request.Username
        };
        
        await userRepository.SaveAsync(user);
        return Ok(user);
    }

    /// <summary>
    /// Update user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated user</returns>
    [HttpPut("user/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Update user", Description = "Update an existing user")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await userRepository.FindById(id);
        if (user == null) return NotFound();

        user.Username = request.Username;
        
        await userRepository.SaveAsync(user);
        return Ok(user);
    }

    /// <summary>
    /// Get user articles
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="start">Cursor string</param>
    /// <param name="limit">Limit</param>
    /// <returns>List of articles</returns>
    [HttpGet("user/{id}/articles")]
    [SwaggerOperation(Summary = "Get user articles", Description = "Get articles by author ID with keyset pagination")]
    public async Task<IActionResult> GetUserArticles(Guid id, [FromQuery] string? start, [FromQuery] uint limit = 10)
    {
        LastSeekCursor? cursor = ParseCursor(start);
        var (articles, hasMore) = await articleRepository.ListByAuthorIdAsync(id, limit, cursor);
        
        string? nextCursor = null;
        if (hasMore && articles.Count > 0)
        {
            var last = articles.Last();
            if (last.CreatedAt.HasValue)
            {
                nextCursor = $"{last.CreatedAt.Value.Ticks}_{last.Id}";
            }
        }

        return Ok(new { Data = articles, NextCursor = nextCursor, HasMore = hasMore });
    }

    private LastSeekCursor? ParseCursor(string? start)
    {
        if (string.IsNullOrEmpty(start)) return null;
        var parts = start.Split('_');
        if (parts.Length != 2) return null;
        if (!long.TryParse(parts[0], out var ticks) || !Guid.TryParse(parts[1], out var id)) return null;
        return new LastSeekCursor(new DateTime(ticks), id);
    }
}
