using System;
using Fakebook.Models.DTOs;

namespace Fakebook.Features.PostFeature;

public class ListPostFeatureRequest
{
  public DateTimeOffset? Before { get; init; } = DateTimeOffset.MaxValue;
  public DateTimeOffset? After { get; init; } = DateTimeOffset.MinValue;
  public ushort Limit { get; init; } = 10;
}

public class ListPostFeatureResponse
{
  public IEnumerable<PostDto> Posts { get; set; } = [];
}
