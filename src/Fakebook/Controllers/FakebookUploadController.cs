using System;
using Fakebook.Features.UploadFeature;
using Fakebook.Helpers;
using Fakebook.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fakebook.Controllers;

[ApiController]
[Route("api/v1/fakebook/upload", Name = "FakebookProfile")]
[Authorize]
public class FakebookUploadController(
  CookiesManager _cookieManager,
  IProfileService _profileService,
  IFileUploadService _fileUploadService
) : FakebookController(_cookieManager, _profileService)
{
  private static void ValidateFile(IFormFile file)
  {
    if (file == null || file.Length == 0)
    {
      throw new Exception("File is empty");
    }

    if (file.Length > 50 * 1024 * 1024)
    {
      throw new Exception("File size exceeds the limit of 50MB");
    }

    var allowedContentTypes = new List<string>
    {
      "image/",
      "video/",
      "audio/",
    };

    var isValidContentType = allowedContentTypes.Any(ct => file.ContentType.StartsWith(ct));
  }

  [HttpPost(Name = "UploadFile")]
  public async Task<IActionResult> UploadFile([FromForm] UploadFileFeatureRequest request)
  {
    var authUserId = GetAuthenticatedUserId();
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    if (profile.UserId != authUserId)
    {
      throw new Exception("User doesn't own this profile");
    }

    var file = request.File;
    ValidateFile(file);

    using var stream = file.OpenReadStream();
    var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var uploadedFile = await _fileUploadService.UploadFile(Enums.UploadMethod.Local, stream, new UploadOptions
    {
      FileName = newFileName,
      Directory = $"{profile.ProfileId}/uploads",
      ContentType = file.ContentType
    });

    var url = await _fileUploadService.GenerateFileUrl(uploadedFile.Path);
    return OkResponse(new UploadFileFeatureResponse
    {
      Name = uploadedFile.Name,
      Path = uploadedFile.Path,
      Size = uploadedFile.Size,
      ContentType = uploadedFile.ContentType,
      Url = url
    });
  }
}
