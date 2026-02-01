namespace Fakebook.Models.DTOs;

public record class CommentCreatorDto
{
  public Guid? Id { get; init; }
  public string? Name { get; init; }
  public string? AvatarUrl { get; init; }
}
