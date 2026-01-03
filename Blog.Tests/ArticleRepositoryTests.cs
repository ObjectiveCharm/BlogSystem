using Blog.Data;
using Blog.Entities;
using Blog.Entities.Types;
using Blog.Repository;
using Blog.Repository.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Blog.Tests;

[TestFixture]
public class ArticleRepositoryTests
{
    private BlogDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task SaveAsync_ShouldUpsertArticle()
    {
        using var context = GetInMemoryDbContext();
        var repository = new ArticleRepository(context);
        var article = new Article { Id = Guid.NewGuid(), Title = "New Article", AuthorId = Guid.NewGuid() };

        // Insert
        await repository.SaveAsync(article);
        Assert.That(await context.Articles.FindAsync(article.Id), Is.Not.Null);

        // Update
        article.Title = "Updated Title";
        await repository.SaveAsync(article);
        var updated = await context.Articles.FindAsync(article.Id);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
    }

    [Test]
    public async Task 
        ChangeStatusAsync_ShouldUpdateStatus()
    {
        using var context = GetInMemoryDbContext();
        var repository = new ArticleRepository(context);
        var id = Guid.NewGuid();
        context.Articles.Add(new Article { Id = id, Title = "T", Status = ArticleStatus.DRAFT, AuthorId = Guid.NewGuid() });
        await context.SaveChangesAsync();

        var oldStatus = await repository.PublishAsync(id);
        Assert.That(oldStatus, Is.EqualTo(ArticleStatus.DRAFT));
        
        var article = await context.Articles.FindAsync(id);
        Assert.That(article, Is.Not.Null);
        Assert.That(article!.Status, Is.EqualTo(ArticleStatus.PUBLISHED));

        oldStatus = await repository.HiddenAsync(id);
        Assert.That(oldStatus, Is.EqualTo(ArticleStatus.PUBLISHED));
        
        article = await context.Articles.FindAsync(id);
        Assert.That(article, Is.Not.Null);
        Assert.That(article!.Status, Is.EqualTo(ArticleStatus.HIDDEN));
    }

    [Test]
    public async Task ListByAuthorIdAsync_ShouldReturnCorrectPage()
    {
        using var context = GetInMemoryDbContext();
        var repository = new ArticleRepository(context);
        var authorId = Guid.NewGuid();
        
        // Create 5 articles with different dates
        var articles = new List<Article>();
        for (int i = 1; i <= 5; i++)
        {
            articles.Add(new Article 
            { 
                Id = Guid.NewGuid(), 
                Title = $"Article {i}", 
                AuthorId = authorId, 
                CreatedAt = DateTime.UtcNow.AddMinutes(-i) // Newer first: A1(newest)..A5(oldest)
            });
        }
        
        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();

        // Page 1: Get 2
        var (page1, hasMore1) = await repository.ListByAuthorIdAsync(authorId, bulkSize: 2);
        Assert.That(page1.Count, Is.EqualTo(2));
        Assert.That(hasMore1, Is.True);
        Assert.That(page1[0].Title, Is.EqualTo("Article 1")); // Newest
        Assert.That(page1[1].Title, Is.EqualTo("Article 2"));

        // Page 2: Get next 2 using cursor from last item of page 1
        var lastItem = page1.Last();
        var cursor = new LastSeekCursor(lastItem.CreatedAt ?? DateTime.MinValue, lastItem.Id);
        
        var (page2, hasMore2) = await repository.ListByAuthorIdAsync(authorId, bulkSize: 2, lastSeekCursor: cursor);
        Assert.That(page2.Count, Is.EqualTo(2));
        Assert.That(hasMore2, Is.True);
        Assert.That(page2[0].Title, Is.EqualTo("Article 3"));
        Assert.That(page2[1].Title, Is.EqualTo("Article 4"));

        // Page 3: Get remaining 1
        lastItem = page2.Last();
        cursor = new LastSeekCursor(lastItem.CreatedAt ?? DateTime.MinValue, lastItem.Id);
        
        var (page3, hasMore3) = await repository.ListByAuthorIdAsync(authorId, bulkSize: 2, lastSeekCursor: cursor);
        Assert.That(page3.Count, Is.EqualTo(1));
        Assert.That(hasMore3, Is.False);
        Assert.That(page3[0].Title, Is.EqualTo("Article 5"));
    }

    [Test]
    public async Task ListAllArticleAsync_ShouldReturnCorrectPage()
    {
        using var context = GetInMemoryDbContext();
        var repository = new ArticleRepository(context);
        var authorId = Guid.NewGuid();
        
        // Create 5 articles with different dates
        var articles = new List<Article>();
        for (int i = 1; i <= 5; i++)
        {
            articles.Add(new Article 
            {
                Id = Guid.NewGuid(), 
                Title = $"Article {i}", 
                AuthorId = authorId, 
                CreatedAt = DateTime.UtcNow.AddMinutes(-i) // Newer first: A1(newest)..A5(oldest)
            });
        }
        
        context.Articles.AddRange(articles);
        await context.SaveChangesAsync();

        // Page 1: Get 2
        var (page1, hasMore1) = await repository.ListAllArticleAsync(bulkSize: 2);
        Assert.That(page1.Count, Is.EqualTo(2));
        Assert.That(hasMore1, Is.True);
        Assert.That(page1[0].Title, Is.EqualTo("Article 1")); // Newest
        Assert.That(page1[1].Title, Is.EqualTo("Article 2"));

        // Page 2: Get next 2 using cursor from last item of page 1
        var lastItem = page1.Last();
        var cursor = new LastSeekCursor(lastItem.CreatedAt ?? DateTime.MinValue, lastItem.Id);
        
        var (page2, hasMore2) = await repository.ListAllArticleAsync(bulkSize: 2, lastSeekCursor: cursor);
        Assert.That(page2.Count, Is.EqualTo(2));
        Assert.That(hasMore2, Is.True);
        Assert.That(page2[0].Title, Is.EqualTo("Article 3"));
        Assert.That(page2[1].Title, Is.EqualTo("Article 4"));

        // Page 3: Get remaining 1
        lastItem = page2.Last();
        cursor = new LastSeekCursor(lastItem.CreatedAt ?? DateTime.MinValue, lastItem.Id);
        
        var (page3, hasMore3) = await repository.ListAllArticleAsync(bulkSize: 2, lastSeekCursor: cursor);
        Assert.That(page3.Count, Is.EqualTo(1));
        Assert.That(hasMore3, Is.False);
        Assert.That(page3[0].Title, Is.EqualTo("Article 5"));
    }
}