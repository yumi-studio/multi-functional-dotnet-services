using System;

namespace Fakebook.Configurations;

public class StorageConfiguration
{
  public required string DirPath { get; set; }
  public required string BaseUrl { get; set; }
  public required string BasePath { get; set; }
}
