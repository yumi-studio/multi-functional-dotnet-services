using System;

namespace YumiStudio.Common.Constants;

public class ExternalProviders
{
  public enum Provider
  {
    Google = 1,
    Facebook = 2,
    Twitter = 3,
    GitHub = 4,
    LinkedIn = 5,
    Microsoft = 6
  }

  public static string GetProviderName(Provider provider)
  {
    return provider switch
    {
      Provider.Google => "Google",
      Provider.Facebook => "Facebook",
      Provider.Twitter => "Twitter",
      Provider.GitHub => "GitHub",
      Provider.LinkedIn => "LinkedIn",
      Provider.Microsoft => "Microsoft",
      _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
    };
  }
}
