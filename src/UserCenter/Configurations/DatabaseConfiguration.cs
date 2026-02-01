using System;

namespace UserApi.Configurations;

public class DatabaseConfiguration
{
  public string Host { get; set; } = "localhost";
  public uint Port { get; set; } = 3306;
  public string Database { get; set; } = string.Empty;
  public string User { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}
