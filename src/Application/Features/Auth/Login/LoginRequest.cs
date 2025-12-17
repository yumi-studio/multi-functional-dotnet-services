using System;
using YumiStudio.Domain.Enums;

namespace YumiStudio.Application.Features.Auth.Login;

public class LoginRequest
{
  public required string Username { get; set; }
  public required string Password { get; set; }
}
