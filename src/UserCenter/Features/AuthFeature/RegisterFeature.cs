using System;
using System.ComponentModel.DataAnnotations;
using UserApi.Enums;

namespace UserApi.Features.AuthFeature;

public class RegisterFeatureRequest
{
  [Required]
  [EmailAddress(ErrorMessage = "Invalid email format")]
  [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
  public required string Email { get; set; }

  [Required]
  [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
  [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
  [RegularExpression(@"^[^\s]+$", ErrorMessage = "Password cannot contain spaces")]
  public required string Password { get; set; }

  [Required]
  [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
  [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
  [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
  public required string Username { get; set; }

  [Required]
  [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
  public required string FirstName { get; set; }

  [Required]
  [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
  public required string LastName { get; set; }

  [Required]
  [DataType(DataType.Date)]
  [Range(typeof(DateOnly), "1900-01-01", "2025-12-31", ErrorMessage = "Birth date must be between 1900-01-01 and 2025-12-31")]
  public required DateOnly BirthDate { get; set; }

  [Required]
  [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender value")]
  public required Gender Gender { get; set; }
}

public class RegisterFeatureResponse
{
  public required Guid UserId { get; set; }
}
