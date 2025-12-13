namespace YumiStudio.YumiDotNet.Application.DTOs.Fakebook;

public record class ProfileDto
{
  public Guid Id { get; init; }
  public string Name { get; init; }
  public string AvatarUrl { get; init; }
}
