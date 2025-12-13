using System;
using System.Threading.Tasks;
using YumiStudio.YumiDotNet.Application.Interfaces.Fakebook;
using YumiStudio.YumiDotNet.Common.Helpers;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

namespace YumiStudio.YumiDotNet.API.Controllers.Fakebook;

public abstract class FakebookController(
  CookiesManager _cookieManager,
  IProfileService _profileService
) : GenericController
{
  private Profile? ActiveProfile { get; set; } = null;
  private bool ActiveProfileLoaded { get; set; } = false;

  /// <summary>
  /// Get active profile if available
  /// </summary>
  /// <returns></returns>
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
}
