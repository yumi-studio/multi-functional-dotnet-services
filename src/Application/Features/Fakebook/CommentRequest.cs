using System;
using YumiStudio.Domain.Enums.Fakebook;

namespace YumiStudio.Application.Features.Fakebook;

public class CommentRequest
{
  public class GetComments
  {
    public DateTimeOffset Before { get; init; } = DateTime.MaxValue;
    public ushort Limit { get; init; } = 5;
  }

  public class CreateComment
  {
    public required string Content { get; init; }
  }

  public class UpdateComment
  {
    public required string Content { get; init; }
  }

  public class ReactComment
  {
    public required ReactionType Type { get; init; }
  }
}
