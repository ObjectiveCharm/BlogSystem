using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Static;

public static class GlobalConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("article_status", new[] { "published", "hidden", "draft" })
            .HasPostgresExtension("pgcrypto");
    }
}
