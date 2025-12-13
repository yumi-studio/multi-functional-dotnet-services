using System;
using Google.Protobuf.WellKnownTypes;

namespace YumiStudio.YumiDotNet.Common.Configurations;

public class LocalDirConfig
{
  /// <summary>
  /// This is root path, ex: /path/to/dir; C:/path/to/dir;
  /// </summary>
  public required string DirPath { get; set; }
  public required string BaseUrl { get; set; }
}

public class AwsS3Config : LocalDirConfig
{
  public required string Region { get; set; }
  public required string BucketName { get; set; }
  public required string AccessKey { get; set; }
  public required string SecretKey { get; set; }
}

public class StorageConfig
{
  public const string LOCALDIR = "LocalDir";
  public const string AWSS3 = "AwsS3";

  public required string Default { get; set; }
  public required LocalDirConfig LocalDir { get; set; }
  public required AwsS3Config AwsS3 { get; set; }
}