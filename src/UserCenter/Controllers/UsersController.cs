using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.Enums;
using UserApi.Exceptions;
using UserApi.Repositories.Interfaces;
using UserApi.Services.Interfaces;

namespace UserApi.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(
  IUserService _userService,
  IUserRepository _userRepository
) : GenericController
{
  // private readonly string _avatarDir = "uploads/user/avatar/";

  [HttpGet("me", Name = "UserMe")]
  [Authorize]
  public async Task<IActionResult> GetMe()
  {
    var userId = Guid.Parse(HttpContext.User.Identity?.Name!);
    var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotExistException();

    string? avatarPath;
    if (user.Avatar == null || user.Avatar.Length == 0)
    {
      avatarPath = user.Gender switch
      {
        Gender.Male => "avatar/men.png",
        Gender.Female => "avatar/women.png",
        _ => "avatar/menwomen.png"
      };
    }
    else
    {
      avatarPath = user.Avatar;
    }

    var userDto = await _userService.GetUserDTOFromUser(user);
    return OkResponse(userDto);
  }

  [HttpPost("avatar", Name = "UploadUserAvatar")]
  public async Task<IActionResult> UploadUserAvatar(IFormFile file)
  {
    var userId = GetAuthenticatedUserId();
    var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotExistException();

    _userRepository.Update(user);
    await _userRepository.SaveChangesAsync();

    return OkResponse();
  }

  [HttpDelete("avatar", Name = "RemoveUserAvatar")]
  public async Task<IActionResult> RemoveUserAvatar()
  {
    var userId = GetAuthenticatedUserId();
    var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotExistException();
    if (user.Avatar == null || user.Avatar.Length == 0)
    {
      throw new Exception("Avatar is not set.");
    }

    return OkResponse();
  }
}
