using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Fakebook.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Models.Entities;

[Table("fakebook_post_media")]
public class PostMedia
{
  [Key]
  [Column("post_media_id")]
  [Required]
  public Guid PostMediaId { get; set; } = Guid.Empty;

  [Column("post_id")]
  [Required]
  public Guid PostId { get; set; } = Guid.Empty;

  [Column("name")]
  [Required]
  public string Name { get; set; } = string.Empty;

  [Column("type")]
  [Required]
  public MediaType FileType { get; set; }

  [Column("content_type")]
  [Required]
  public string ContentType { get; set; } = string.Empty;

  [Column("path")]
  [Required]
  public string Path { get; set; } = string.Empty;

  [Column("size")]
  [Required]
  public double Size { get; set; } = 0.00;

  public Post Post { get; set; } = null!;
}
