using Blog.Controllers;
using Blog.Data;
using Blog.DTOs;
using Blog.Entities;
using Blog.Entities.Types;
using Blog.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Blog.Tests.ControllerTests;

[TestFixture]
public class ArticleControllerTests
{
    private BlogDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task CreateArticle_ShouldReturnOk_WhenRequestIsValid()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var articleRepo = new ArticleRepository(context);
        var tagRepo = new TagRepository(context);
        
        var tagId = Guid.NewGuid();
        await tagRepo.SaveAsync(new Tag { Id = tagId, Name = "TestTag", CreatedAt = DateTime.UtcNow });
        
        var controller = new ArticleController(articleRepo, tagRepo);

        var request = new CreateArticleRequest(
            Title: "Test Article",
            Content: "Test Content",
            AuthorId: Guid.NewGuid(),
            TagIds: new List<Guid> { tagId }
        );

        var result = await controller.CreateArticle(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.TypeOf<Article>());
        var returnedArticle = (Article)okResult.Value;
        Assert.That(returnedArticle.Title, Is.EqualTo(request.Title));
    }

    [Test]
    public async Task GetArticles_ShouldReturnList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var articleRepo = new ArticleRepository(context);
        var tagRepo = new TagRepository(context);
        var controller = new ArticleController(articleRepo, tagRepo);

        context.Articles.Add(new Article
        {
            Id = Guid.NewGuid(),
            Title = "Article 1",
            Content = "Content",
            AuthorId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = ArticleStatus.PUBLISHED
        });
        await context.SaveChangesAsync();

        var result = await controller.GetArticles(null, 10);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task GetArticle_ShouldReturnArticle_WhenIdExists()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var articleRepo = new ArticleRepository(context);
        var tagRepo = new TagRepository(context);
        var controller = new ArticleController(articleRepo, tagRepo);

        var id = Guid.NewGuid();
        context.Articles.Add(new Article
        {
            Id = id,
            Title = "Article 1",
            Content = "Content",
            AuthorId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = ArticleStatus.PUBLISHED
        });
        await context.SaveChangesAsync();

        var result = await controller.GetArticle(id);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var article = (Article)okResult.Value;
        Assert.That(article.Id, Is.EqualTo(id));
    }

    [Test]
    public async Task UpdateArticle_ShouldUpdateFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var articleRepo = new ArticleRepository(context);
        var tagRepo = new TagRepository(context);
        var controller = new ArticleController(articleRepo, tagRepo);

        var id = Guid.NewGuid();
        context.Articles.Add(new Article
        {
            Id = id,
            Title = "Old Title",
            Content = "Old Content",
            AuthorId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = ArticleStatus.DRAFT
        });
        await context.SaveChangesAsync();

        var request = new UpdateArticleRequest("New Title", "New Content", new List<Guid>());
        var result = await controller.UpdateArticle(id, request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var article = (Article)okResult.Value;
        Assert.That(article.Title, Is.EqualTo("New Title"));
        
        var dbArticle = await context.Articles.FindAsync(id);
        Assert.That(dbArticle.Title, Is.EqualTo("New Title"));
    }

    [Test]
    public async Task ChangeStatus_ShouldUpdateStatus()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var articleRepo = new ArticleRepository(context);
        var tagRepo = new TagRepository(context);
        var controller = new ArticleController(articleRepo, tagRepo);

        var id = Guid.NewGuid();
        context.Articles.Add(new Article
        {
            Id = id,
            Title = "Title",
            Content = "Content",
            AuthorId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = ArticleStatus.DRAFT
        });
        await context.SaveChangesAsync();

        var result = await controller.ChangeStatus(id, ArticleStatus.PUBLISHED);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        
        var dbArticle = await context.Articles.FindAsync(id);
        Assert.That(dbArticle.Status, Is.EqualTo(ArticleStatus.PUBLISHED));
    }
}
