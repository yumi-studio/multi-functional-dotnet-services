using System;
using Fakebook.Configurations;
using Fakebook.Enums;
using Microsoft.Extensions.Options;

namespace Fakebook.Infrastructure.UploadMethods;

/// <summary>
/// Implementation of local upload method
/// </summary>
public class LocalUploadMethod(
  IOptions<StorageConfiguration> storageConfiguration
) : IUploadMethod
{
  private readonly StorageConfiguration _storageConfiguration = storageConfiguration.Value;

  public bool CanHandle(UploadMethod method)
         => method == UploadMethod.Local;

  public Task<bool> DeleteAsync(string filePath)
  {
    try
    {
      var absoluteFilePath = Path.Combine(_storageConfiguration.DirPath, filePath);
      if (File.Exists(absoluteFilePath))
      {
        File.Delete(absoluteFilePath);
        return Task.FromResult(true);
      }
      return Task.FromResult(false);
    }
    catch
    {
      return Task.FromResult(false);
    }
  }

  public Task<string> GetUrlAsync(string filePath)
  {
    return Task.FromResult(
      _storageConfiguration.BaseUrl.TrimEnd('/')
      + "/" + _storageConfiguration.BasePath.Trim('/')
      + "/" + filePath.Replace("\\", "/").Trim('/')
    );
  }

  public async Task<string> UploadAsync(Stream fileStream, string fileName, string subDirectory = "")
  {
    var relativeFilePath = Path.Combine(subDirectory, fileName);
    var absoluteDirPath = Path.Combine(_storageConfiguration.DirPath, subDirectory);
    var absoluteFilePath = Path.Combine(_storageConfiguration.DirPath, relativeFilePath);

    // Ensure directory exists
    Directory.CreateDirectory(absoluteDirPath);

    using var fs = File.Create(absoluteFilePath);
    await fileStream.CopyToAsync(fs);

    return relativeFilePath;
  }
}
