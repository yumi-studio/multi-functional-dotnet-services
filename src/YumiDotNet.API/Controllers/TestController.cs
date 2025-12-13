using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YumiStudio.YumiDotNet.Application.Features.Auth.Register;
using YumiStudio.YumiDotNet.Application.Interfaces;
using YumiStudio.YumiDotNet.Domain.Enums;
using YumiStudio.YumiDotNet.Domain.Interfaces;

namespace YumiStudio.YumiDotNet.API.Controllers;

[Route("api/test")]
[ApiController]
public class TestController(
  IUserService _userService,
  IUserRepository _userRepository,
  IFileUploadService _fileUploadService
) : GenericController
{
  [HttpGet("create-users", Name = "TestCreateUsers")]
  [AllowAnonymous]
  public async Task<IActionResult> CreateTestUsers()
  {
    // Create user1
    var userDto1 = await _userService.RegisterUser(new RegisterRequest()
    {
      Email = "user1@exmaple.com",
      Username = "user1",
      Password = "admin123",
      FirstName = "User1",
      LastName = "Test",
      Gender = Gender.Male,
      BirthDate = DateOnly.FromDateTime(DateTime.Parse("1997-01-01"))
    });

    // Create user2
    var userDto2 = await _userService.RegisterUser(new RegisterRequest()
    {
      Email = "user2@exmaple.com",
      Username = "user2",
      Password = "admin123",
      FirstName = "User2",
      LastName = "Test",
      Gender = Gender.Female,
      BirthDate = DateOnly.FromDateTime(DateTime.Parse("1999-12-12"))
    });

    // Grant user1 as system admin
    var user1 = await _userRepository.GetByIdAsync(userDto1.Id);
    if (user1 != null)
    {
      user1.IsSystemAdmin = true;
      _userRepository.Update(user1);
      await _userRepository.SaveChangesAsync();
    }

    return OkResponse(new { userDto1, userDto2 });
  }

  [HttpPost("upload-file", Name = "TestUploadFile")]
  [AllowAnonymous]
  public async Task<IActionResult> UploadFile(IFormFile file)
  {
    var fileUpload = await _fileUploadService.Upload(file);
    return OkResponse(fileUpload);
  }
}