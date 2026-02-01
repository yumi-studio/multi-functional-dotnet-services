namespace Fakebook.Models.DTOs;

public record class CommentStatisticDto
{
  public ReactionDto Reactions { get; init; } = new ReactionDto { Upvote = 0, Downvote = 0 };
  public int Replies { get; init; }
}
