using System;

namespace YumiStudio.API.Middlewares;

public class AuthorizeMiddleware(RequestDelegate next, ILogger<AuthorizeMiddleware> logger)
{

  private readonly RequestDelegate _next = next;
  private readonly ILogger<AuthorizeMiddleware> _logger = logger;

  public async Task Invoke(HttpContext context)
  {
    await _next(context);
  }
}
