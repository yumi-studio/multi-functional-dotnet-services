using System.Net;
using System.Text.Json;
using YumiStudio.YumiDotNet.Application.DTOs;
using YumiStudio.YumiDotNet.Domain.Exceptions;

namespace YumiStudio.YumiDotNet.API.Middlewares;

public class RequestErrorMiddleware(RequestDelegate next, ILogger<RequestErrorMiddleware> logger)
{
  private readonly RequestDelegate _next = next;
  private readonly ILogger<RequestErrorMiddleware> _logger = logger;

  public async Task Invoke(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (UnauthorizedException ex)
    {
      _logger.LogError(ex, "Unauthorized exception caught in middleware");
      await HandleUnauthorizedAsync(context);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception caught in middleware");
      await HandleExceptionAsync(context, ex);
    }
  }

  private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
  {
    const HttpStatusCode code = HttpStatusCode.InternalServerError;
    var result = JsonSerializer.Serialize(new ResponseDto<string>
    {
      Success = false,
      Message = "An unexpected error occurred.",
      Errors = [ex.Message]
    });

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)code;

    await context.Response.WriteAsync(result);
  }

  private static async Task HandleUnauthorizedAsync(HttpContext context, UnauthorizedException? ex = null)
  {
    var result = JsonSerializer.Serialize(new ResponseDto<string>
    {
      Success = false,
      Message = ex == null ? "User is unauthorized" : ex.Message,
    });

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

    await context.Response.WriteAsync(result);
  }
}
