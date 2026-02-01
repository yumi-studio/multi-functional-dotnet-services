using System;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Services;

public class PostService(
  IPostRepository _postRepository,
  IPostCommentRepository _postCommentRepository,
  IReactionRepository _reactionRepository
) : IPostService
{
  public async Task<IEnumerable<Post>> GetFeedAsync(DateTimeOffset? before, int limit)
  {
    return await _postRepository.GetPostsAsync(null, before ?? DateTimeOffset.UtcNow, limit);
  }

  /// <summary>
  /// Fetch posts visible to a specific profile
  /// </summary>
  /// <param name="profileId"></param>
  /// <param name="before"></param>
  /// <param name="limit"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  public async Task<IEnumerable<Post>> GetFeedByProfileAsync(Guid profileId, DateTimeOffset? before, int limit)
  {
    var postDbSet = _postRepository.GetDbSet();
    var createdBefore = before ?? DateTimeOffset.MaxValue;
    var createdAfter = DateTimeOffset.MinValue;
    var query = postDbSet.AsQueryable();
    query = query.Where(p => p.CreatedBy == profileId);
    query = query.Where(p => p.CreatedAt >= createdAfter && p.CreatedAt <= createdBefore);
    query = query.OrderByDescending(p => p.CreatedAt).Take(limit);

    return await query.ToListAsync();
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
