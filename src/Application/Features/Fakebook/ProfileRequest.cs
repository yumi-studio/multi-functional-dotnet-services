namespace YumiStudio.Application.Features.Fakebook;

public record class ProfileRequest
{
  public record class Create
  {
    public string? Name { get; init; }
  }

  public record class UpdateAvatar
  {
    public IFormFile? File { get; init; }
  }
}
