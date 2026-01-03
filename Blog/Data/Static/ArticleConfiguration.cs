using Blog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Static;

public static class ArticleConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Author).WithMany(p => p.Articles)
                .HasConstraintName("articles_author_id_fkey")
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
