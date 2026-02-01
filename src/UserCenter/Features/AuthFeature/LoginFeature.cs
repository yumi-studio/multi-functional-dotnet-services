using System;

namespace UserApi.Features.AuthFeature;

public class LoginFeatureRequest
{
  public required string Email { get; set; }
  public required string Password { get; set; }
}

public class LoginFeatureResponse
{
  public required string Token { get; set; }
}
