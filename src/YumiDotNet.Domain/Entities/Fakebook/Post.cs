using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using YumiStudio.YumiDotNet.Domain.Enums.Fakebook;

namespace YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

[Table("fakebook_posts")]
public class Post
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; } = Guid.Empty;

  [Column("content")]
  public string Content { get; set; } = string.Empty;

  [Required]
  [Column("created_by")]
  public Guid CreatedBy { get; set; }

  [Required]
  [Column("visibility")]
  public PostVisibility Visibility { get; set; } = PostVisibility.Public;

  [Required]
  [Column("created_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Required]
  [Column("updated_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

  #region navigation property

  public Profile Profile { get; set; } = null!;

  #endregion
}
