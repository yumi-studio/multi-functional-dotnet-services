namespace Fakebook.Models.DTOs;

public record class ReactionDto
{
  public int Upvote { get; init; } = 0;
  public int Downvote { get; init; } = 0;
}
