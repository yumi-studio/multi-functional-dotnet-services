namespace YumiStudio.Application.Features.Fakebook;

public record class ProfileRequest
{
  public record class Create
  {
    public string? Name { get; init; }
  }
}
