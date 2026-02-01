using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UserApi.Enums;

namespace UserApi.Models;

[Table("user_externals")]
public class UserExternal
{
  [Column("id")]
  [Required]
  [Key]
  public Guid Id { get; set; }

  [Column("user_id")]
  [Required]
  public Guid UserId { get; set; }

  [Column("provider")]
  [Required]
  public AuthProvider Provider { get; set; }

  [Column("provider_user_id")]
  [Required]
  public string? ProviderUserId { get; set; }

  [Column("linked_at")]
  [Required]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

  public User? User { get; set; }
}
