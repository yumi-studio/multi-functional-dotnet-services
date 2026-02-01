namespace Fakebook.Models.DTOs;

public record class ProfileDto
{
  public Guid Id { get; init; }
  public string? Name { get; init; }
  public string? AvatarUrl { get; init; }

  public static ProfileDto FromEntity(Entities.Profile profile, string? avatarUrl)
  {
    return new ProfileDto
    {
      Id = profile.ProfileId,
      Name = profile.Name,
      AvatarUrl = avatarUrl
    };
  }
}
