using System;

namespace Fakebook.Features.PostFeature;

public class UploadMediaFeatureRequest
{
  public required IFormFile MediaItem { get; init; }
}

public class UploadMediaFeatureResponse
{

  public Guid? FileUploadId { get; set; }
}
