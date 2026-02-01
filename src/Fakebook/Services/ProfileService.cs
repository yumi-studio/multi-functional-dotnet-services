using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Services;

public class ProfileService(
  IFileUploadService fileUploadService,
  IProfileRepository profileRepository
) : IProfileService
{
  private readonly IFileUploadService _fileUploadService = fileUploadService;
  private readonly IProfileRepository _profileRepository = profileRepository;

  public async Task<List<Profile>> GetAllProfiles(Guid userId)
  {
    var profiles = await _profileRepository.GetDbSet().Where(p => p.UserId == userId).ToListAsync();
    return profiles;
  }

  public async Task<Profile> CreateProfile(Guid userId, NewProfileDto newProfileDto)
  {
    var profile = await _profileRepository.GetByIdAsync(userId);
    var newProfile = new Profile()
    {
      ProfileId = profile == null ? userId : Guid.NewGuid(),
      UserId = userId,
      Name = newProfileDto.Name
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
