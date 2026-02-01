using System;

namespace Fakebook.Helpers;

public class CookiesManager(
  IHttpContextAccessor _httpContextAccessor
)
{
  public const string FAKEBOOK_PROFILE_ACTIVE = "fakebook_profile_active";

  public string? GetCookie(string key)
  {
    if (_httpContextAccessor.HttpContext == null) return null;
    return _httpContextAccessor.HttpContext?.Request.Cookies[key];
  }

  public void SetCookie(string key, string value, DateTimeOffset? duration = null)
  {
    if (_httpContextAccessor.HttpContext == null) return;

    _httpContextAccessor.HttpContext?.Response.Cookies.Append(key, value, new CookieOptions()
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.None,
      Path = "/",
      Expires = duration ?? DateTimeOffset.UtcNow.AddYears(1)
    });
  }

  public Guid? GetFakebookActiveProfile()
  {
    var value = GetCookie(FAKEBOOK_PROFILE_ACTIVE);
    return value == null ? null : Guid.Parse(value);
  }

  public void SetFakebookActiveProfile(Guid profileId)
  {
    if (_httpContextAccessor.HttpContext == null) return;

    SetCookie(FAKEBOOK_PROFILE_ACTIVE, profileId.ToString(), DateTimeOffset.UtcNow.AddYears(999));
  }
}
