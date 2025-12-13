using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

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
  public string Name { get; set; }

  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  #region navigation property

  public User User { get; set; } = null!;

  #endregion
}
