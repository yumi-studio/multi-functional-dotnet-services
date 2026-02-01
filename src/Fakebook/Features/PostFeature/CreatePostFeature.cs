using System;
using System.ComponentModel.DataAnnotations;
using Fakebook.Enums;

namespace Fakebook.Features.PostFeature;

public class UploadedFileDimension
{
  public double Width { get; set; } = 0.00;
  public double Height { get; set; } = 0.00;
}

public class UploadedFile
{
  public string Name { get; set; } = string.Empty;
  public string ContentType { get; set; } = string.Empty;
  public string Path { get; set; } = string.Empty;
  public string Url { get; set; } = string.Empty;
  public double Size { get; set; } = 0.00;
}

public class UploadedImage : UploadedFile
{
  public UploadedFileDimension Dimension { get; set; } = new UploadedFileDimension() { Width = 0, Height = 0 };
}

public class CreatePostFeatureRequest
{

  public string? Content { get; init; }

  [EnumDataType(typeof(PostVisibility))]
  public PostVisibility Visibility { get; init; }

  public List<UploadedFile> MediaItems { get; init; } = [];
}

public class CreatePostFeatureResponse
{
  public Guid? Id { get; set; }
}
