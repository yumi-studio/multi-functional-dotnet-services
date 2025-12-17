using YumiStudio.Domain.Enums.Fakebook;

namespace YumiStudio.Application.DTOs.Fakebook;

public record class CommentDto
{
  public Guid? Id { get; init; }

  public string? Content { get; init; }

  public DateTimeOffset? CreatedAt { get; init; }

  public DateTimeOffset? UpdatedAt { get; init; }

  public CommentCreatorDto? Creator { get; init; }

  public CommentStatisticDto? Statistic { get; init; }

  public ReactionType? Reaction { get; init; }

}
