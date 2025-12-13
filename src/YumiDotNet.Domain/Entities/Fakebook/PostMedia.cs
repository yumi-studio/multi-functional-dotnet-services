using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

[Table("fakebook_post_media")]
[Index(nameof(PostId), nameof(FileId), IsUnique = true, Name = "idx_post_id_file_id")]
public class PostMedia
{
  [Key]
  [Column("post_media_id")]
  public Guid PostMediaId { get; set; } = Guid.Empty;

  [Column("post_id")]
  public Guid PostId { get; set; } = Guid.Empty;

  [Column("file_id")]
  public Guid FileId { get; set; } = Guid.Empty;

  #region navigation property

  public Post Post { get; set; } = null!;
  public FileUpload FileUpload { get; set; } = null!;

  #endregion
}
