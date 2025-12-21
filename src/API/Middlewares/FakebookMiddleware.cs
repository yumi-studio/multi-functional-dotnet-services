using System;

namespace YumiStudio.API.Middlewares;

public class FakebookMiddleware(RequestDelegate _next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    // var path = context.Request.Path.Value?.ToLower();

    // if (path != null && path.StartsWith("/fakebook"))
    // {
    //   // Logic riêng cho fakebook
    // }

    await _next(context);
  }
}
