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
