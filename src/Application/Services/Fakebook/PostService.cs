using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Application.Interfaces.Fakebook;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;

namespace YumiStudio.Application.Services.Fakebook;

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
