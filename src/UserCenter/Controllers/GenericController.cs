using Microsoft.AspNetCore.Mvc;

namespace UserApi.Controllers;

public abstract class GenericController : ControllerBase
{
  protected IActionResult OkResponse()
  {
    return OkResponse<object?>(null);
  }

  protected IActionResult OkResponse<T>(T data)
  {
    return Ok(new
    {
      Success = true,
      Data = data
    });
  }

  protected IActionResult ErrorResponse(List<string> errors)
  {
    return BadRequest(new
    {
      Success = false,
      Errors = errors
    });
  }

  protected Guid GetAuthenticatedUserId()
  {
    return Guid.Parse(HttpContext.User.Identity?.Name!);
  }
}
