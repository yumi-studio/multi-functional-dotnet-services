using System;
using Fakebook.Enums;

namespace Fakebook.Infrastructure.UploadMethods;

/// <summary>
/// Interface for upload methods
/// </summary>
public interface IUploadMethod
{
  /// <summary>
  /// Check if this upload method can handle the given method
  /// </summary>
  public bool CanHandle(UploadMethod method);

  /// <summary>
  /// Upload the file stream to a specific sub-directory and return the file path or URL
  /// </summary>
  public Task<string> UploadAsync(Stream fileStream, string fileName, string subDirectory = "");
}
