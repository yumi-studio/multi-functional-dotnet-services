using Microsoft.EntityFrameworkCore;
using YumiStudio.Application.Interfaces.Fakebook;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Exceptions;
using YumiStudio.Domain.Interfaces;
using YumiStudio.Domain.Interfaces.Fakebook;
using FakebookDtos = YumiStudio.Application.DTOs.Fakebook;

namespace YumiStudio.Application.Services.Fakebook;

public class ProfileService(
  IProfileRepository _profileRepository,
  IUserRepository _userRepository
) : IProfileService
{
  public async Task<IEnumerable<Profile>> GetAllProfiles(Guid userId)
  {
    // throw new NotImplementedException();
    var profiles = await _profileRepository.GetDbSet().Where(p => p.UserId == userId).ToArrayAsync();
    return profiles;
  }

  public async Task<Profile> CreateProfile(Guid userId, FakebookDtos.NewProfileDto newProfileDto)
  {
    var user = await _userRepository.GetByIdAsync(userId) ?? throw new UserNotExistException();
    var profile = await _profileRepository.GetByIdAsync(userId);
    var newProfile = new Profile()
    {
      ProfileId = profile == null ? user.Id : Guid.NewGuid(),
      UserId = user.Id,
      Name = newProfileDto.Name ?? $"{user.FirstName} {user.LastName}"
    };
    await _profileRepository.AddAsync(newProfile);
    await _profileRepository.SaveChangesAsync();

    return newProfile;
  }

  public async Task<Profile?> GetProfileById(Guid userId)
  {
    var profile = await _profileRepository.GetByIdAsync(userId);
    return profile;
  }
}
