using System;
using Microsoft.AspNetCore.Authorization;
using YumiStudio.Common.Constants;
using YumiStudio.Common.Helpers;
using YumiStudio.Domain.Exceptions;
using YumiStudio.Domain.Interfaces;

namespace YumiStudio.API.Middlewares;

public class CheckTokenBlacklistMiddleware(
  RequestDelegate next,
  ILogger<AuthorizeMiddleware> _logger
)
{
  private readonly RequestDelegate _next = next;

  public async Task Invoke(HttpContext context)
  {
    _logger.LogInformation($"CheckTokenBlacklistMiddleware: Start");
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
          _logger.LogInformation($"CheckTokenBlacklistMiddleware: Token is expired");
          throw new UnauthorizedException();
        }
      }
    }
    _logger.LogInformation($"CheckTokenBlacklistMiddleware: End");

    await _next(context);
  }
}
