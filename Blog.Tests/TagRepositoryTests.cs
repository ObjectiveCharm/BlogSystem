using Blog.Data;
using Blog.Entities;
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
public class TagRepositoryTests
{
    private BlogDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new BlogDbContext(options);
    }

    [Test]
    public async Task SaveAsync_ShouldUpsertTag()
    {
        using var context = GetInMemoryDbContext();
        var repository = new TagRepository(context);
        var id = Guid.NewGuid();
        var tag = new Tag { Id = id, Name = "C#" };

        // Insert
        await repository.SaveAsync(tag);
        var stored = await context.Tags.FindAsync(id);
        Assert.That(stored, Is.Not.Null);
        Assert.That(stored!.Name, Is.EqualTo("C#"));

        // Update
        tag.Name = ".NET";
        await repository.SaveAsync(tag);
        stored = await context.Tags.FindAsync(id);
        Assert.That(stored!.Name, Is.EqualTo(".NET"));
    }

    [Test]
    public async Task FindByIdAsync_ShouldReturnTag()
    {
        using var context = GetInMemoryDbContext();
        var repository = new TagRepository(context);
        var id = Guid.NewGuid();
        context.Tags.Add(new Tag { Id = id, Name = "Test" });
        await context.SaveChangesAsync();

        var result = await repository.FindByIdAsync(id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo("Test"));
    }

    [Test]
    public async Task ListAllTagsAsync_ShouldReturnCorrectPage()
    {
        using var context = GetInMemoryDbContext();
        var repository = new TagRepository(context);
        
        // Create 5 tags with different dates
        var tags = new List<Tag>();
        for (int i = 1; i <= 5; i++)
        {
            tags.Add(new Tag 
            { 
                Id = Guid.NewGuid(), 
                Name = $"Tag {i}", 
                CreatedAt = DateTime.UtcNow.AddMinutes(-i) // Newer first
            });
        }
        
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        // Page 1: Get 2
        var (page1, hasMore1) = await repository.ListAllTagsAsync(bulkSize: 2);
        Assert.That(page1.Count, Is.EqualTo(2));
        Assert.That(hasMore1, Is.True);
        Assert.That(page1[0].Name, Is.EqualTo("Tag 1")); // Newest
        Assert.That(page1[1].Name, Is.EqualTo("Tag 2"));

        // Page 2: Get next 2
        var lastItem = page1.Last();
        var cursor = new LastSeekCursor(lastItem.CreatedAt ?? DateTime.MinValue, lastItem.Id);
        
        var (page2, hasMore2) = await repository.ListAllTagsAsync(bulkSize: 2, lastSeekCursor: cursor);
        Assert.That(page2.Count, Is.EqualTo(2));
        Assert.That(hasMore2, Is.True);
        Assert.That(page2[0].Name, Is.EqualTo("Tag 3"));
        Assert.That(page2[1].Name, Is.EqualTo("Tag 4"));

        // Page 3: Get remaining 1
        lastItem = page2.Last();
        cursor = new LastSeekCursor(lastItem.CreatedAt ?? DateTime.MinValue, lastItem.Id);
        
        var (page3, hasMore3) = await repository.ListAllTagsAsync(bulkSize: 2, lastSeekCursor: cursor);
        Assert.That(page3.Count, Is.EqualTo(1));
        Assert.That(hasMore3, Is.False);
        Assert.That(page3[0].Name, Is.EqualTo("Tag 5"));
    }
}
