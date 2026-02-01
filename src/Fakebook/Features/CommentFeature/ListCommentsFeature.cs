using System;

namespace Fakebook.Features.CommentFeature;

public class ListCommentsFeatureRequest
{
  public DateTimeOffset Before { get; init; } = DateTime.MaxValue;
  public ushort Limit { get; init; } = 5;
}

public class ListCommentsFeatureResponse
{

}
