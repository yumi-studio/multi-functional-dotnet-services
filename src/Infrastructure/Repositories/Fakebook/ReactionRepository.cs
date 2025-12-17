using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Enums.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories.Fakebook;

public class ReactionRepository(
  AppDbContext appDbContext
) : BaseRepository<Reaction, Guid>(appDbContext), IReactionRepository
{
  public async Task<ReactionDto> CountByCommentId(Guid commentId)
  {
    var countUpvote = await _dbSet
      .Where(e => e.TargetType == ReactionTargetType.Comment)
      .Where(e => e.TargetId == commentId)
      .Where(e => e.ReactionType == ReactionType.UpVote)
      .Select(e => e.Id)
      .ToListAsync();

    var countDownvote = await _dbSet
      .Where(e => e.TargetType == ReactionTargetType.Comment)
      .Where(e => e.TargetId == commentId)
      .Where(e => e.ReactionType == ReactionType.DownVote)
      .Select(e => e.Id)
      .ToListAsync();

    return new ReactionDto { Upvote = countUpvote.Count, Downvote = countDownvote.Count };
  }

  public async Task<ReactionDto> CountByPostId(Guid postId)
  {
    var countUpvote = await _dbSet
      .Where(e => e.TargetType == ReactionTargetType.Post)
      .Where(e => e.TargetId == postId)
      .Where(e => e.ReactionType == ReactionType.UpVote)
      .Select(e => e.Id)
      .ToListAsync();

    var countDownvote = await _dbSet
      .Where(e => e.TargetType == ReactionTargetType.Post)
      .Where(e => e.TargetId == postId)
      .Where(e => e.ReactionType == ReactionType.DownVote)
      .Select(e => e.Id)
      .ToListAsync();

    return new ReactionDto { Upvote = countUpvote.Count, Downvote = countDownvote.Count };
  }

  public async Task<Reaction?> GetReactionByProfileId(Guid profileId, ReactionTargetType targetType, Guid targetId)
  {
    return await _dbSet.Where(e => e.TargetType == targetType && e.TargetId == targetId && e.ReactedBy == profileId)
      .FirstOrDefaultAsync();
  }

  public async Task<IEnumerable<Reaction>> GetReactionsByProfileId(Guid profileId, ReactionTargetType targetType, IEnumerable<Guid> targetIds)
  {
    return await _dbSet.Where(e => e.TargetType == targetType && targetIds.Contains(e.TargetId) && e.ReactedBy == profileId).ToListAsync();
  }
}
