using System;
using YumiStudio.YumiDotNet.Domain.Enums;

namespace YumiStudio.YumiDotNet.Application.Features.Auth.Login;

public class LoginRequest
{
  public required LoginType LoginType { get; set; }
  public string? Username { get; set; }
  public string? Email { get; set; }
  public string? PhoneNumber { get; set; }
  public string? Password { get; set; }
  public string? FacebookToken { get; set; }
  public string? GoogleToken { get; set; }
  public string? TwitterToken { get; set; }
  public string? MicrosoftToken { get; set; }

}
