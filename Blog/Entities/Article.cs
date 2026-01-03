using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blog.Entities.Types;
using Microsoft.EntityFrameworkCore;

namespace Blog.Entities;

[Table("articles")]
[Index(nameof(AuthorId), Name = "idx_articles_author_id")]
public partial class Article
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    [Column("title")]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [Column("content")]
    public string? Content { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
    
    [Column("status")]
    public ArticleStatus Status { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Articles")]
    public virtual User Author { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
