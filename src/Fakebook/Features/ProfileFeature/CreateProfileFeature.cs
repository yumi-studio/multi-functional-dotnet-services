using System;

namespace Fakebook.Features.ProfileFeature;

public class CreateProfileFeatureRequest
{
  public required string Name { get; init; }
}

public class CreateProfileFeatureResponse
{
  public Guid ProfileId { get; init; }
}
