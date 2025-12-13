using System;
using YumiStudio.YumiDotNet.Application.DTOs.Fakebook;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

namespace YumiStudio.YumiDotNet.Application.Interfaces.Fakebook;

public interface IProfileService
{
  public Task<IEnumerable<Profile>> GetAllProfiles(Guid userId);

  public Task<Profile> CreateProfile(Guid userId, NewProfileDto newProfileData);

  public Task<Profile?> GetProfileById(Guid userId);
}
