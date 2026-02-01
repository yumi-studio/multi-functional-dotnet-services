using System;
using Fakebook.Enums;

namespace Fakebook.Features.PostFeature;

public class ReactPostFeatureRequest
{
  public required ReactionType Type { get; init; }
}

public class ReactPostFeatureResponse
{
  public Guid? Id { get; set; }
}
