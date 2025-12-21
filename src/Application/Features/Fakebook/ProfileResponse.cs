namespace YumiStudio.Application.Features.Fakebook;

public record class ProfileResponse
{
  public record class Create
  {
    public Guid ProfileId { get; init; }
  }
}
