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
  /// <param name="method"></param>
  /// <returns></returns>
  public bool CanHandle(UploadMethod method);

  /// <summary>
  /// Upload the file stream to a specific sub-directory and return the file path or URL
  /// </summary>
  /// <param name="fileStream"></param>
  /// <param name="fileName"></param>
  /// <param name="subDirectory"></param>
  /// <returns></returns>
  public Task<string> UploadAsync(Stream fileStream, string fileName, string subDirectory = "");

  /// <summary>
  /// Delete file
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  public Task<bool> DeleteAsync(string filePath);

  /// <summary>
  /// Get file url
  /// </summary>
  /// <param name="filePath"></param>
  /// <returns></returns>
  public Task<string> GetUrlAsync(string filePath);
}
