using System;

namespace YumiStudio.Application.Features.Auth.Login;

public class LoginRequest
{
  public required string Email { get; set; }
  public required string Password { get; set; }
}
