using System;
using System.ComponentModel.DataAnnotations;
using Fakebook.Enums;

namespace Fakebook.Features.PostFeature;

public class UpdatePostFeatureRequest
{
  [EnumDataType(typeof(PostVisibility))]
  public PostVisibility Visibility { get; init; }
  public string? Content { get; init; }
}

public class UpdatePostFeatureResponse
{
  public Guid? Id { get; set; }
}
