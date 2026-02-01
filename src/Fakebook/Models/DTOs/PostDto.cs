using Fakebook.Enums;

namespace Fakebook.Models.DTOs;

public record PostDto
{
  public Guid Id { get; init; }
  public string? Content { get; init; }
  public PostVisibility Visibility { get; init; }
  public List<MediaItemDto>? MediaItems { get; init; }
  public DateTimeOffset CreatedAt { get; init; }
  public PostCreatorDto? Creator { get; init; }
  public PostStatisticDto? Statistic { get; init; }
  public ReactionType? Reaction { get; init; }
}
