using System;

namespace YumiStudio.YumiDotNet.Application.DTOs.Fakebook;

public record MediaItemDto
{
  public Guid Id { get; init; }
  public string Name { get; init; }
  public string Path { get; init; }
  public string Source { get; init; }
  public string Type { get; init; }
}
