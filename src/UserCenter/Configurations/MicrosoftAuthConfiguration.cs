using System;

namespace UserApi.Configurations;

public class MicrosoftAuthConfiguration
{
  public string ClientId { get; set; } = string.Empty;
  public string ClientSecret { get; set; } = string.Empty;
  public string AuthorizationEndpoint { get; set; } = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
  public string TokenEndpoint { get; set; } = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
}
