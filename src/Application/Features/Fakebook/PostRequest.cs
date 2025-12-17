using System;
using System.ComponentModel.DataAnnotations;
using YumiStudio.Domain.Enums.Fakebook;

namespace YumiStudio.Application.Features.Fakebook;

public class PostRequest
{
  public class GetPosts()
  {
    public DateTimeOffset? Before { get; init; } = DateTimeOffset.MaxValue;
    public DateTimeOffset? After { get; init; } = DateTimeOffset.MinValue;
    public ushort Limit { get; init; } = 10;
  }

  public class UploadMedia
  {
    public required IFormFile MediaItem { get; init; }
  }

  public class CreatePost
  {
    public string? Content { get; init; }

    [EnumDataType(typeof(PostVisibility))]
    public PostVisibility Visibility { get; init; }

    public List<Guid>? MediaItemIds { get; init; }
  }

  public class UpdatePost
  {
    public string? Content { get; init; }
  }

  public class UpdatePostContent
  {
    public string? Content { get; init; }
  }

  public class ReactPost
  {
    public required ReactionType Type { get; init; }
  }

  public class SharePost
  {
    public string? Content { get; init; }
  }
}
