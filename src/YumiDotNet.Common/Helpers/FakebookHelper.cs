using System;
using Microsoft.Extensions.Options;
using YumiStudio.YumiDotNet.Common.Configurations;

namespace YumiStudio.YumiDotNet.Common.Helpers;

public class FakebookHelper(
  IOptions<StorageConfig> _storageConfig,
  AwsS3Helper _awsS3Helper
)
{
  /// <summary>
  /// Resolve profile avatar, return default if not available
  /// </summary>
  /// <param name="path"></param>
  /// <returns></returns>
  public async Task<string> GetProfileAvatar(string path = "fakebook/profiles/sample.jpg")
  {
    if (_storageConfig.Value.Default == StorageConfig.LOCALDIR)
    {
      return _storageConfig.Value.LocalDir.BaseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    if (_storageConfig.Value.Default == StorageConfig.AWSS3)
    {
      return await _awsS3Helper.GetFileUrlAsync(path);
    }

    return "";
  }
}
