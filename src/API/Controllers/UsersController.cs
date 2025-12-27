using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YumiStudio.Application.Interfaces;
using YumiStudio.Application.Services;
using YumiStudio.Domain.Exceptions;
using YumiStudio.Domain.Interfaces;

namespace YumiStudio.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController(
  IUserService _userService,
  IFileUploadService _fileUploadService,

  IUserRepository _userRepository
) : GenericController
{
  private readonly string _avatarDir = "uploads/user/avatar/";

  [HttpGet("me", Name = "UserMe")]
  [Authorize]
  public async Task<IActionResult> GetMe()
  {
    var userId = Guid.Parse(HttpContext.User.Identity?.Name!);
    var userDto = await _userService.GetUserById(userId);
    return OkResponse(userDto);
  }

  [HttpPost("avatar", Name = "UpdateAvatar")]
  [Authorize]
  public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile fileUpload)
  {
    var userId = GetAuthenticatedUserId();
    var file = fileUpload ?? throw new Exception("No file uploaded");
    var fileCustomOption = new FileCustomOption { SaveDirPath = _avatarDir + userId, UploaderId = GetAuthenticatedUserId() };
    var savedFile = await _fileUploadService.Upload(file, fileCustomOption);

    var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotExistException();
    user.Avatar = savedFile.Path;
    _userRepository.Update(user);
    await _userRepository.SaveChangesAsync();

    var avatarUrl = await _fileUploadService.GetFileUrl(savedFile.Path);

    return OkResponse(new
    {
      AvatarUrl = avatarUrl
    });
  }

}
