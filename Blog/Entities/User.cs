using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blog.Entities;

[Table("users")]
[Index(nameof(Username), Name = "users_username_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    [MaxLength(50)]
    public string Username { get; set; } = null!;
    
    

    [InverseProperty("Author")]
    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    [InverseProperty("User")]
    public virtual UserCredential? UserCredential { get; set; }
}
