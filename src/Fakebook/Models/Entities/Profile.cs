using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fakebook.Models.Entities;

[Table("fakebook_profiles")]
public class Profile
{
  [Column("profile_id")]
  [Key]
  [Required]
  public Guid ProfileId { get; set; }

  [Column("user_id")]
  [Required]
  public Guid UserId { get; set; }

  [Column("name")]
  [Required]
  public required string Name { get; set; }

  [Column("avatar")]
  public string? Avatar { get; set; }

  [Column("created_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
