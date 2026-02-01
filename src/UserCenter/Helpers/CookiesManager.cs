using System;

namespace UserApi.Helpers;

public class CookiesManager(
  IHttpContextAccessor _httpContextAccessor
)
{
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
}
