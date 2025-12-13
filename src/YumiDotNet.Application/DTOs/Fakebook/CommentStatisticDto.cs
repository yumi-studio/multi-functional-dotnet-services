namespace YumiStudio.YumiDotNet.Application.DTOs.Fakebook;

public record class CommentStatisticDto
{
  public ReactionDto Reactions { get; init; } = new ReactionDto { Upvote = 0, Downvote = 0 };
  public int Replies { get; init; }
}
