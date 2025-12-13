using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YumiStudio.YumiDotNet.Application.Interfaces;

namespace YumiStudio.YumiDotNet.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(
  IUserService _userService
) : GenericController
{
  [HttpGet("me", Name = "UserMe")]
  [Authorize]
  public async Task<IActionResult> GetMe()
  {
    var userId = Guid.Parse(HttpContext.User.Identity?.Name!);
    var userDto = await _userService.GetUserById(userId);
    return OkResponse(userDto);
  }
}
