using System;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Services.Interfaces;

public interface IProfileService
{
  public Task<List<Profile>> GetAllProfiles(Guid userId);

  public Task<Profile> CreateProfile(Guid userId, NewProfileDto newProfileData);

  public Task<Profile?> GetProfileById(Guid userId);
}
