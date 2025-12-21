using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Entities;
using YumiStudio.Domain.Enums;

namespace YumiStudio.Domain.Entities;

[Table("file_uploads")]
public class FileUpload
{
  [Key]
  [Column("id")]
  public Guid Id { get; set; } = Guid.Empty;

  [Required]
  [Column("name")]
  public string Name { get; set; } = string.Empty;

  [Required]
  [Column("path")]
  public string Path { get; set; } = string.Empty;

  [Required]
  [Column("type")]
  public FileUploadType FileType { get; set; } = FileUploadType.Unknown;

  [Required]
  [Column("mime")]
  public string Mime { get; set; } = string.Empty;

  [Column("aws_s3_key")]
  public string AwsS3Key { get; set; } = string.Empty;

  [Required]
  [Column("uploaded_by")]
  public Guid UploadedBy { get; set; } = Guid.Empty;

  [Required]
  [Column("is_draft")]
  [Comment("New upload is marked as draft and be cleared if not change to true")]
  public bool IsDraft { get; set; } = true;

  [Column("created_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("updated_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

  #region navigation property

  public User Uploader { get; set; } = null!;
  public Fakebook.PostMedia? MediaItem { get; set; }

  #endregion
}
