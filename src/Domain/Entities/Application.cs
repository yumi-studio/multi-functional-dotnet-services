using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YumiStudio.Domain.Entities;

[Table("applications")]
public class Application
{
  [Key]
  [Column("id")]
  public required string Id { get; set; }

  [Column("name")]
  public required string Name { get; set; }

  [Column("description")]
  public required string Description { get; set; }

  [Column("icon")]
  public required string Icon { get; set; }

  [Column("is_active")]
  public required bool IsActive { get; set; }

  [Column("created_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("updated_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
