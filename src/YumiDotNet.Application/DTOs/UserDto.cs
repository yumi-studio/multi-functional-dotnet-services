using YumiStudio.YumiDotNet.Domain.Enums;

namespace YumiStudio.YumiDotNet.Application.DTOs;

public class UserDto
{
  public Guid Id { get; set; } = Guid.Empty;
  public string Username { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string FirstName { get; set; } = string.Empty;
  public string LastName { get; set; } = string.Empty;
  public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTimeOffset.Parse("1900-01-01T00:00:00+00:00").DateTime);
  public Gender Gender { get; set; } = Gender.Unknown;
  public string? Bio { get; set; } = string.Empty;
  public string? Avatar { get; set; } = string.Empty;
  public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.Parse("1900-01-01T00:00:00+00:00");
}