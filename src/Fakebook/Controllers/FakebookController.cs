using System;
using Fakebook.Helpers;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Services.Interfaces;

namespace Fakebook.Controllers;

public abstract class FakebookController(
  CookiesManager _cookieManager,
  IProfileService _profileService
) : GenericController
{
  private Profile? ActiveProfile { get; set; } = null;
  private bool ActiveProfileLoaded { get; set; } = false;

  protected async Task<Profile?> GetActiveProfile()
  {
    if (ActiveProfileLoaded) return ActiveProfile;

    var activeProfileId = _cookieManager.GetFakebookActiveProfile();
    if (activeProfileId == null)
    {
      ActiveProfileLoaded = true;
      return null;
    }

    var profile = await _profileService.GetProfileById((Guid)activeProfileId);
    if (profile != null && profile.UserId == GetAuthenticatedUserId())
    {
      ActiveProfile = profile;
    }

    ActiveProfileLoaded = true;
    return ActiveProfile;
  }

  protected async Task CanHandleProfile()
  {
    var authUserId = GetAuthenticatedUserId();
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    if (profile.UserId != authUserId)
    {
      throw new Exception("User doesn't own this profile");
    }
  }
}
