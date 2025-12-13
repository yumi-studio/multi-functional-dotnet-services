using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using YumiStudio.YumiDotNet.Domain.Enums;

namespace YumiStudio.YumiDotNet.Domain.Entities;

[Table("users")]
[Comment("Table containing user information")]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Username), IsUnique = true)]
public class User
{
  [Key]
  [Column("id")]
  [Comment("Primary key for the User table")]
  public Guid Id { get; set; } = Guid.Empty;

  [Required]
  [MaxLength(50)]
  [Column("username")]
  [Comment("Username of the user")]
  public string Username { get; set; } = string.Empty;

  [Required]
  [EmailAddress]
  [Column("email")]
  [Comment("Email address of the user")]
  public string Email { get; set; } = string.Empty;

  [Required]
  [MaxLength(50)]
  [Column("first_name")]
  [Comment("First name of the user")]
  public string FirstName { get; set; } = string.Empty;

  [Required]
  [MaxLength(50)]
  [Column("last_name")]
  [Comment("Last name of the user")]
  public string LastName { get; set; } = string.Empty;

  [Required]
  [Column("gender")]
  [Comment("Gender of the user")]
  public Gender Gender { get; set; } = Gender.Unknown;

  [Required]
  [DataType(DataType.Date)]
  [Column("birth_date")]
  [Comment("Birth date of the user")]
  public DateOnly BirthDate { get; set; } = DateOnly.Parse("1900-01-01");

  [MaxLength(500)]
  [Column("bio")]
  [Comment("Short biography of the user")]
  public string? Bio { get; set; } = string.Empty;

  [MaxLength(255)]
  [Column("avatar")]
  [Comment("URL or path to the user's avatar image")]
  public string? Avatar { get; set; } = string.Empty;

  [Required]
  [Column("password_hash")]
  [Comment("Hashed password of the user")]
  public string PasswordHash { get; set; } = string.Empty;

  [Required]
  [Column("is_system_admin")]
  [Comment("Indicates if the user is a system administrator")]
  public bool IsSystemAdmin { get; set; } = false;

  [Required]
  [Column("joined_at")]
  [Comment("Date and time when the user joined")]
  public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("created_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
  public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

  [Column("updated_at")]
  [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
  public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}