namespace Fakebook.Models.DTOs;

public record class PostStatisticDto
{
  public int Comment { get; init; }
  public int Share { get; init; }
  public ReactionDto Reactions { get; init; } = new ReactionDto { Upvote = 0, Downvote = 0 };
}
