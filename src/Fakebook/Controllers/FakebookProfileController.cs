using System;
using Fakebook.Features.ProfileFeature;
using Fakebook.Helpers;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fakebook.Enums;
using Fakebook.Repositories.Interfaces;

namespace Fakebook.Controllers;

[ApiController]
[Route("api/v1/fakebook/profiles", Name = "FakebookProfile")]
[Authorize]
public class FakebookProfileController(
  CookiesManager cookiesManager,
  IProfileService profileService,
  IFileUploadService fileUploadService,
  IProfileRepository profileRepository
) : FakebookController(
  cookiesManager,
  profileService
)
{
  private readonly CookiesManager _cookiesManager = cookiesManager;
  private readonly IProfileService _profileService = profileService;
  private readonly IFileUploadService _fileUploadService = fileUploadService;
  private readonly IProfileRepository _profileRepository = profileRepository;

  #region Profile

  [HttpGet(Name = "ListProfiles")]
  public async Task<IActionResult> ListProfiles()
  {
    var authUserId = GetAuthenticatedUserId();
    var profiles = await _profileService.GetAllProfiles(authUserId);
    List<ProfileDto> profilesDto = [];
    foreach (var profile in profiles)
    {
      var avatarUrl = profile.Avatar == null
        ? null
        : await _fileUploadService.GenerateFileUrl(profile.Avatar);
      profilesDto.Add(ProfileDto.FromEntity(profile, avatarUrl));
    }
    return OkResponse(profilesDto);
  }

  [HttpGet("me", Name = "Current Profile")]
  public async Task<IActionResult> Me()
  {
    await CanHandleProfile();
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var avatarUrl = profile.Avatar == null
      ? null
      : await _fileUploadService.GenerateFileUrl(profile.Avatar);

    return OkResponse(ProfileDto.FromEntity(profile, avatarUrl));
  }

  [HttpGet("switch/{id}", Name = "SwitchProfile")]
  public async Task<IActionResult> SwitchProfile(Guid id)
  {
    var authUserId = GetAuthenticatedUserId();
    var profile = await _profileService.GetProfileById(id) ?? throw new Exception("Profile is not exist");
    if (profile.UserId != authUserId)
    {
      throw new Exception("User doesn't own this profile");
    }

    _cookiesManager.SetFakebookActiveProfile(profile.ProfileId);
    return OkResponse();
  }

  [HttpPost(Name = "CreateProfile")]
  public async Task<IActionResult> CreateProfile([FromBody] CreateProfileFeatureRequest request)
  {
    Guid userId = GetAuthenticatedUserId();
    var newProfileDto = new NewProfileDto()
    {
      Name = request.Name
    };
    var newProfile = await _profileService.CreateProfile(userId, newProfileDto);
    return OkResponse(new CreateProfileFeatureResponse()
    {
      ProfileId = newProfile.ProfileId
    });
  }

  [HttpPost("avatar", Name = "UpdateAvatar")]
  public async Task<IActionResult> UpdateAvatar(IFormFile file)
  {
    await CanHandleProfile();
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");

    var stream = file.OpenReadStream();
    var newFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var uploadedAvatar = await _fileUploadService.UploadFile(stream, new UploadOptions
    {
      FileName = newFileName,
      Directory = $"{profile.ProfileId}/avatars",
      ContentType = file.ContentType
    });

    profile.Avatar = uploadedAvatar.Path;
    _profileRepository.Update(profile);
    await _profileRepository.SaveChangesAsync();

    var avatarUrl = await _fileUploadService.GenerateFileUrl(uploadedAvatar.Path);
    return OkResponse(avatarUrl);
  }

  #endregion
}
