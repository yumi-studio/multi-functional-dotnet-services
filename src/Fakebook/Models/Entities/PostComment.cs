using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Fakebook.Models.Entities;

[Table("fakebook_post_comments")]
public class PostComment
{
  [Column("id")]
  [Key]
  public Guid Id { get; set; }

  [Column("content")]
  [Required]
  public string Content { get; set; } = string.Empty;

  [Column("created_by")]
  [Required]
  public Guid CreatedBy { get; set; }

  [Column("post_id")]
  [Required]
  public Guid PostId { get; set; }

  [Column("created_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("updated_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

  #region navigation property

  public Profile Profile { get; set; } = null!;
  public Post Post { get; set; } = null!;

  #endregion
}
