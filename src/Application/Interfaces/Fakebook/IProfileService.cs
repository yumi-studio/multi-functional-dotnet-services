using System;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Domain.Entities.Fakebook;

namespace YumiStudio.Application.Interfaces.Fakebook;

public interface IProfileService
{
  public Task<IEnumerable<Profile>> GetAllProfiles(Guid userId);

  public Task<Profile> CreateProfile(Guid userId, NewProfileDto newProfileData);

  public Task<Profile?> GetProfileById(Guid userId);
}
