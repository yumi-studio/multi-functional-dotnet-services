using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Application.Features.Fakebook;
using YumiStudio.Application.Interfaces.Fakebook;
using YumiStudio.Common.Helpers;

namespace YumiStudio.API.Controllers.Fakebook;

[ApiController]
[Route("api/v1/fakebook/profiles", Name = "FakebookProfile")]
public class FakebookProfileController(
  IProfileService _profileService,
  CookiesManager _cookiesManager,
  FakebookHelper _fakebookHelper
) : GenericController
{
  #region Profile

  [HttpGet(Name = "ListProfiles")]
  [Authorize]
  public async Task<IActionResult> ListProfiles()
  {
    var authUserId = GetAuthenticatedUserId();
    var profiles = await _profileService.GetAllProfiles(authUserId) ?? throw new Exception("Active profile is not exist");
    List<ProfileDto> profilesDto = [];
    foreach (var profile in profiles)
    {
      profilesDto.Add(new ProfileDto
      {
        Id = profile.ProfileId,
        Name = profile.Name,
        AvatarUrl = await _fakebookHelper.GetProfileAvatar()
      });
    }
    return OkResponse(profilesDto);
  }

  [HttpGet("me", Name = "Current Profile")]
  [Authorize]
  public async Task<IActionResult> Me()
  {
    var authUserId = GetAuthenticatedUserId();
    var activeProfileId = _cookiesManager.GetFakebookActiveProfile() ?? throw new Exception("No active profile");
    var profile = await _profileService.GetProfileById(activeProfileId) ?? throw new Exception("Active profile is not exist");
    if (profile.UserId != authUserId)
    {
      throw new Exception("User doesn't own this profile");
    }

    return OkResponse(new
    {
      Id = profile.ProfileId,
      Name = profile.Name,
      AvatarUrl = await _fakebookHelper.GetProfileAvatar()
    });
  }

  [HttpGet("switch/{id}", Name = "SwitchProfile")]
  [Authorize]
  public async Task<IActionResult> SwitchProfile(Guid id)
  {
    var authUserId = GetAuthenticatedUserId();
    var profile = await _profileService.GetProfileById(id) ?? throw new Exception("Profile is not exist");
    if (profile.UserId != authUserId)
    {
      throw new Exception("User doesn't own this profile");
    }

    _cookiesManager.SetFakebookActiveProfile(profile.ProfileId);
    return OkResponse<object?>(null);
  }

  [HttpPost(Name = "CreateProfile")]
  [Authorize]
  public async Task<IActionResult> CreateProfile([FromBody] ProfileRequest.Create request)
  {
    Guid userId = GetAuthenticatedUserId();
    var newProfileDto = new NewProfileDto()
    {
      Name = request.Name
    };
    var newProfile = await _profileService.CreateProfile(userId, newProfileDto);
    return OkResponse(new ProfileResponse.Create()
    {
      ProfileId = newProfile.ProfileId
    });
  }

  #endregion
}
