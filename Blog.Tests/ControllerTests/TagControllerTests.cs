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
public class TagControllerTests
{
    private BlogDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task CreateTag_ShouldReturnOk()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var tagRepo = new TagRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new TagController(tagRepo, articleRepo);

        var request = new CreateTagRequest("New Tag");
        var result = await controller.CreateTag(request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var tag = (Tag)okResult.Value;
        Assert.That(tag.Name, Is.EqualTo("New Tag"));
    }

    [Test]
    public async Task GetTags_ShouldReturnList()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var tagRepo = new TagRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new TagController(tagRepo, articleRepo);

        context.Tags.Add(new Tag { Id = Guid.NewGuid(), Name = "Tag1", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var result = await controller.GetTags(null, 10);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task UpdateTag_ShouldUpdateName()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var tagRepo = new TagRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new TagController(tagRepo, articleRepo);

        var id = Guid.NewGuid();
        context.Tags.Add(new Tag { Id = id, Name = "Old Name", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var request = new UpdateTagRequest("New Name");
        var result = await controller.UpdateTag(id, request);

        Assert.That(result, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)result;
        var tag = (Tag)okResult.Value;
        Assert.That(tag.Name, Is.EqualTo("New Name"));
    }

    [Test]
    public async Task GetArticlesByTag_ShouldReturnArticles()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = GetDbContext(dbName);
        var tagRepo = new TagRepository(context);
        var articleRepo = new ArticleRepository(context);
        var controller = new TagController(tagRepo, articleRepo);

        var tagId = Guid.NewGuid();
        var tag = new Tag { Id = tagId, Name = "TargetTag", CreatedAt = DateTime.UtcNow };
        context.Tags.Add(tag);

        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = "Tagged Article",
            Content = "Content",
            AuthorId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        article.Tags.Add(tag);
        context.Articles.Add(article);
        await context.SaveChangesAsync();

        var result = await controller.GetArticlesByTag(tagId, null, 10);

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
