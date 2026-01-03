using System;
using System.Collections.Generic;
using Blog.Data.Static;
using Blog.Entities;
using Microsoft.EntityFrameworkCore;

namespace Blog.Data;

public partial class BlogDbContext : DbContext
{
    public BlogDbContext()
    {
    }

    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCredential> UserCredentials { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost:5433;Database=blog;Username=alfheim_admin;password=password");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        GlobalConfiguration.Configure(modelBuilder);
        ArticleConfiguration.Configure(modelBuilder);
        TagConfiguration.Configure(modelBuilder);
        UserConfiguration.Configure(modelBuilder);
        UserCredentialConfiguration.Configure(modelBuilder);
        ArticleTagConfiguration.Configure(modelBuilder);
    }

}
