using Blog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Static;

public static class ArticleTagConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>()
            .HasMany(d => d.Tags)
            .WithMany(p => p.Articles)
            .UsingEntity<Dictionary<string, object>>(
                "ArticleTag",
                r => r.HasOne<Tag>().WithMany()
                    .HasForeignKey("TagId")
                    .HasConstraintName("article_tags_tag_id_fkey"),
                l => l.HasOne<Article>().WithMany()
                    .HasForeignKey("ArticleId")
                    .HasConstraintName("article_tags_article_id_fkey"),
                j =>
                {
                    j.HasKey("ArticleId", "TagId").HasName("article_tags_pkey");
                    j.ToTable("article_tags");
                    j.IndexerProperty<Guid>("ArticleId").HasColumnName("article_id");
                    j.IndexerProperty<Guid>("TagId").HasColumnName("tag_id");
                });
    }
}
