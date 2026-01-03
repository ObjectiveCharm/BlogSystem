using Blog.DTOs;
using Blog.Entities;
using Blog.Entities.Types;
using Blog.Repository;
using Blog.Repository.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Blog.Controllers;

[ApiController]
[Route("")]
public class ArticleController(
    ArticleRepository articleRepository,
    TagRepository tagRepository) : ControllerBase
{
    /// <summary>
    /// Get articles with keyset pagination
    /// </summary>
    /// <param name="start">Cursor string (Ticks_Guid)</param>
    /// <param name="limit">Number of items to return</param>
    /// <returns>List of articles</returns>
    [HttpGet("articles")]
    [SwaggerOperation(Summary = "Get articles list", Description = "Get articles with keyset pagination")]
    public async Task<IActionResult> GetArticles([FromQuery] string? start, [FromQuery] uint limit = 10)
    {
        LastSeekCursor? cursor = ParseCursor(start);
        var (articles, hasMore) = await articleRepository.ListAllArticleAsync(limit, cursor);
        
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

    /// <summary>
    /// Get article by ID
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <returns>Article details</returns>
    [HttpGet("article/{id}")]
    [SwaggerOperation(Summary = "Get article", Description = "Get article by ID")]
    public async Task<IActionResult> GetArticle(Guid id)
    {
        var article = await articleRepository.FindByIdAsync(id);
        if (article == null) return NotFound();
        return Ok(article);
    }

    /// <summary>
    /// Create article
    /// </summary>
    /// <param name="request">Article creation request</param>
    /// <returns>Created article</returns>
    [HttpPost("article")]
    [Authorize]
    [SwaggerOperation(Summary = "Create article", Description = "Create a new article")]
    public async Task<IActionResult> CreateArticle([FromBody] CreateArticleRequest request)
    {
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            AuthorId = request.AuthorId, 
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ArticleStatus.DRAFT
        };
        
        if (request.TagIds != null && request.TagIds.Count > 0)
        {
            foreach (var tagId in request.TagIds)
            {
                var tag = await tagRepository.FindByIdAsync(tagId);
                if (tag != null)
                {
                    article.Tags.Add(tag);
                }
            }
        }

        await articleRepository.SaveAsync(article);
        return Ok(article);
    }

    /// <summary>
    /// Update article
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated article</returns>
    [HttpPut("article/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Update article", Description = "Update an existing article")]
    public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] UpdateArticleRequest request)
    {
        var article = await articleRepository.FindByIdAsync(id);
        if (article == null) return NotFound();

        article.Title = request.Title;
        article.Content = request.Content;
        article.UpdatedAt = DateTime.UtcNow;
        // Note: Tags are not updated here as Repository.SaveAsync does not handle collection updates
        
        await articleRepository.SaveAsync(article);
        return Ok(article);
    }

    /// <summary>
    /// Change article status
    /// </summary>
    /// <param name="id">Article ID</param>
    /// <param name="status">New status</param>
    /// <returns>Old status</returns>
    [HttpPatch("article/{id}")]
    [Authorize]
    [SwaggerOperation(Summary = "Change status", Description = "Change article status to PUBLISHED or HIDDEN")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromQuery] ArticleStatus status)
    {
        if (status == ArticleStatus.PUBLISHED)
        {
            var res = await articleRepository.PublishAsync(id);
            return res.HasValue ? Ok(res) : NotFound();
        }
        else if (status == ArticleStatus.HIDDEN)
        {
            var res = await articleRepository.HiddenAsync(id);
            return res.HasValue ? Ok(res) : NotFound();
        }
        return BadRequest("Invalid status. Allowed: 1 (Published), 2 (Hidden)");
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