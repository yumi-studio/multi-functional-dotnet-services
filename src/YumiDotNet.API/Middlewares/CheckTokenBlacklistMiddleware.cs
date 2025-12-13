using System;
using Microsoft.AspNetCore.Authorization;
using YumiStudio.YumiDotNet.Common.Constants;
using YumiStudio.YumiDotNet.Common.Helpers;
using YumiStudio.YumiDotNet.Domain.Exceptions;
using YumiStudio.YumiDotNet.Domain.Interfaces;

namespace YumiStudio.YumiDotNet.API.Middlewares;

public class CheckTokenBlacklistMiddleware(
  RequestDelegate next,
  ILogger<AuthorizeMiddleware> _logger
)
{
  private readonly RequestDelegate _next = next;

  public async Task Invoke(HttpContext context)
  {
    var tokenRepo = context.RequestServices.GetRequiredService<ITokenRepository>();
    var endpoint = context.GetEndpoint();
    var requiresAuth = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null;
    var allowsAnonymous = endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
    if (requiresAuth && !allowsAnonymous)
    {
      if (context.User.Identity != null && context.User.Identity.IsAuthenticated == true)
      {
        var jwtId = (context.User.FindFirst(CustomClaims.JwtId)?.Value) ?? throw new UnauthorizedException();
        var token = await tokenRepo.GetByIdAsync(Guid.Parse(jwtId)) ?? throw new UnauthorizedException();
        if (token.ExpiredAt <= DateTimeOffset.UtcNow)
        {
          throw new UnauthorizedException();
        }
      }
    }

    await _next(context);
  }
}
