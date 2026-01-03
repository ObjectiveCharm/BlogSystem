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
public class TagController(
    TagRepository tagRepository,
    ArticleRepository articleRepository) : ControllerBase
{
    /// <summary>
    /// Get tags with keyset pagination
    /// </summary>
    /// <param name="start">Cursor string (Ticks_Guid)</param>
    /// <param name="limit">Number of items to return</param>
    /// <returns>List of tags</returns>
    [HttpGet("tags")]
    [SwaggerOperation(Summary = "Get tags list", Description = "Get tags with keyset pagination")]
    public async Task<IActionResult> GetTags([FromQuery] string? start, [FromQuery] uint limit = 10)
    {
        LastSeekCursor? cursor = ParseCursor(start);
        var (tags, hasMore) = await tagRepository.ListAllTagsAsync(limit, cursor);
        
        string? nextCursor = null;
        if (hasMore && tags.Count > 0)
        {
            var last = tags.Last();
            if (last.CreatedAt.HasValue)
            {
                nextCursor = $"{last.CreatedAt.Value.Ticks}_{last.Id}";
            }
        }

        return Ok(new { Data = tags, NextCursor = nextCursor, HasMore = hasMore });
    }

    /// <summary>
    /// Create tag
    /// </summary>
    /// <param name="request">Tag creation request</param>
    /// <returns>Created tag</returns>
    [HttpPost("tag")]
    [Authorize]
    [SwaggerOperation(Summary = "Create tag", Description = "Create a new tag")]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };
        await tagRepository.SaveAsync(tag);
        return Ok(tag);
    }

    /// <summary>
    /// Update tag
    /// </summary>
    /// <param name="id">Tag ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated tag</returns>
    [HttpPut("tag/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Update tag", Description = "Update an existing tag")]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagRequest request)
    {
        var tag = await tagRepository.FindByIdAsync(id);
        if (tag == null) return NotFound();

        tag.Name = request.Name;
        // CreatedAt should probably stay same
        
        await tagRepository.SaveAsync(tag);
        return Ok(tag);
    }

    /// <summary>
    /// Get articles by tag
    /// </summary>
    /// <param name="tagId">Tag ID</param>
    /// <param name="start">Cursor string</param>
    /// <param name="limit">Limit</param>
    /// <returns>List of articles</returns>
    [HttpGet("tag/articles")]
    [SwaggerOperation(Summary = "Get articles by tag", Description = "Get articles with keyset pagination for a specific tag")]
    public async Task<IActionResult> GetArticlesByTag([FromQuery] Guid tagId, [FromQuery] string? start, [FromQuery] uint limit = 10)
    {
        LastSeekCursor? cursor = ParseCursor(start);
        var (articles, hasMore) = await articleRepository.ListByTagAsync(tagId, limit, cursor);
        
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
