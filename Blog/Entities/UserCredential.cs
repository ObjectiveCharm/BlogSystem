using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Blog.Entities;

[Table("user_credentials")]
[Index(nameof(Email), Name = "idx_user_credentials_email")]
[Index(nameof(Email), Name = "user_credentials_email_key", IsUnique = true)]
public partial class UserCredential
{
    [Key]
    [Column("user_id")]
    [ForeignKey("User")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid UserId { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("last_changed_at")]
    public DateTime? LastChangedAt { get; set; }
    
    [Column("email_confirmed")]
    public bool EmailConfirmed { get; set; }

    public virtual User User { get; set; } = null!;
}
