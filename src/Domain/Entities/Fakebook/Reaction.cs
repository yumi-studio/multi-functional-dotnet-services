using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Enums.Fakebook;

namespace YumiStudio.Domain.Entities.Fakebook;

[Table("fakebook_reactions")]
[Index(nameof(TargetType), nameof(TargetId))]
[Index(nameof(ReactedBy), nameof(TargetType), nameof(TargetId), IsUnique = true)]
public class Reaction
{
  [Key]
  [Column("id")]
  [Required]
  public Guid Id { get; set; }

  [Column("target_type")]
  [Required]
  public ReactionTargetType TargetType { get; set; }

  [Column("target_id")]
  [Required]
  public Guid TargetId { get; set; }

  [Column("reacted_by")]
  [Required]
  public Guid ReactedBy { get; set; }

  [Column("reaction_type")]
  [Required]
  public ReactionType ReactionType { get; set; } = ReactionType.Unknown;

  [Column("created_at")]
  [Required]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  #region navigation

  public Profile Profile { get; set; } = null!;
    
  #endregion
}
