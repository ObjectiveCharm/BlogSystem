using Blog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Static;

public static class UserConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
        });
    }
}
