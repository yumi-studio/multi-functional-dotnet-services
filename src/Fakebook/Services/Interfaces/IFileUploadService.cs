using System;
using Fakebook.Enums;
using Fakebook.Infrastructure.UploadMethods;

namespace Fakebook.Services.Interfaces;

public class UploadOptions
{
  /// <summary>
  /// Target filename
  /// </summary>
  public required string FileName { get; set; }
  /// <summary>
  /// Target directory within the storage
  /// </summary>
  public required string Directory { get; set; }
  /// <summary>
  /// Content type of the file, e.g. "image/png", "video/mp4", etc.
  /// </summary>
  public required string ContentType { get; set; }
}

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
  public double Size { get; set; } = 0.00;
}

public class UploadedImage : UploadedFile
{
  public UploadedFileDimension Dimension { get; set; } = new UploadedFileDimension() { Width = 0, Height = 0 };
}

public class UploadedVideo : UploadedFile
{
  public UploadedFileDimension Dimension { get; set; } = new UploadedFileDimension() { Width = 0, Height = 0 };
  public double Duration { get; set; } = 0.00;
}

public class UploadedAudio : UploadedFile
{
  public double Duration { get; set; } = 0.00;
}

public interface IFileUploadService
{
  public Task<UploadedFile> UploadFile(Stream fileStream, UploadOptions options);
  public Task<bool> DeleteFile(string path);
  public Task<string> GenerateFileUrl(string path);
}
