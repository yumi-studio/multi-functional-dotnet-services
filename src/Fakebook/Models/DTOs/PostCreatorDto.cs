namespace Fakebook.Models.DTOs;

public record class PostCreatorDto
{
  public Guid? Id { get; init; }
  public string? Name { get; init; }
  public string? AvatarUrl { get; init; }
}
