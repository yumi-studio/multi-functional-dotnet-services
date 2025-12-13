using Microsoft.AspNetCore.Mvc;
using YumiStudio.YumiDotNet.Application.DTOs;

namespace YumiStudio.YumiDotNet.API.Controllers;

public abstract class GenericController : ControllerBase
{
  protected IActionResult OkResponse<T>(T data)
  {
    return Ok(new ResponseDto<T>
    {
      Success = true,
      Message = "Request successful.",
      Data = data
    });
  }

  protected IActionResult ErrorResponse(List<string> errors)
  {
    return BadRequest(new ResponseDto<string>
    {
      Success = false,
      Message = "Request failed.",
      Errors = errors
    });
  }

  protected Guid GetAuthenticatedUserId()
  {
    return Guid.Parse(HttpContext.User.Identity?.Name!);
  }
}
