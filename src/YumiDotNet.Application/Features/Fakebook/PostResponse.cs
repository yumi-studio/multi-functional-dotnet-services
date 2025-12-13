using System;
using YumiStudio.YumiDotNet.Application.DTOs;

namespace YumiStudio.YumiDotNet.Application.Features.Fakebook;

public class PostResponse
{
  public class GetPosts
  {
    public IEnumerable<PostDto> Posts { get; set; } = [];
  }

  public class UploadMedia
  {
    public Guid? FileUploadId { get; set; }
  }

  public class CreatePost
  {
    public Guid? PostId { get; set; }
  }

  public class UpdatePost
  {

  }

  public class DeletePost
  {

  }

  public class ReactPost
  {

  }

  public class SharePost
  {

  }
}

public class Pagination
{
}
