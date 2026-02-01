using System;
using Fakebook.Enums;

namespace Fakebook.Features.CommentFeature;

public class ReactCommentFeatureRequest
{
  public required ReactionType Type { get; init; }
}

public class ReactCommentFeatureResponse
{

}
