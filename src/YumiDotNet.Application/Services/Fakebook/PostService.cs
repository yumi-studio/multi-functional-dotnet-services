using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.YumiDotNet.Application.DTOs.Fakebook;
using YumiStudio.YumiDotNet.Application.Interfaces.Fakebook;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;
using YumiStudio.YumiDotNet.Domain.Interfaces.Fakebook;

namespace YumiStudio.YumiDotNet.Application.Services.Fakebook;

public class PostService(
  IPostRepository _postRepository,
  IPostCommentRepository _postCommentRepository,
  IReactionRepository _reactionRepository
) : IPostService
{
  public async Task<IEnumerable<Post>> GetFeedAsync(DateTimeOffset? before, int limit)
  {
    var createdBefore = before ?? DateTimeOffset.UtcNow;

    return await _postRepository.GetPostsAsync(null, before ?? DateTimeOffset.UtcNow, limit);
  }

  public async Task<PostStatisticDto> GetPostStatistic(Guid id)
  {
    return new PostStatisticDto
    {
      Reactions = await _reactionRepository.CountByPostId(id),
      Comment = await _postCommentRepository.CountByPostId(id),
      Share = 0,
    };
  }
}
