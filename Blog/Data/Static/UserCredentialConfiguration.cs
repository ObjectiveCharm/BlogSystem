using Blog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data.Static;

public static class UserCredentialConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserCredential>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastChangedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithOne(p => p.UserCredential)
                .HasConstraintName("user_credentials_user_id_fkey");
        });
    }
}
