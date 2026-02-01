using System;

namespace Fakebook.Features.UploadFeature;

public class UploadFileFeatureRequest
{
  public required IFormFile File { get; set; }
}

public class UploadFileFeatureResponse
{
  public string Name { get; set; } = string.Empty;
  public string Path { get; set; } = string.Empty;
  public double Size { get; set; } = 0.00;
  public string ContentType { get; set; } = string.Empty;
  public string Url { get; set; } = string.Empty;
}
