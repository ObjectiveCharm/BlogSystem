using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blog.Entities;

[Table("tags")]
[Index(nameof(Name), Name = "tags_name_key", IsUnique = true)]
public partial class Tag
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }
    
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
