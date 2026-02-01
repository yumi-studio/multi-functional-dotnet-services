using System;
using Fakebook.Enums;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Services;

public class CommentService(
  // IProfileRepository _profileRepository,
  IPostCommentRepository _postCommentRepository,
  IReactionRepository _reactionRepository
) : ICommentService
{
  public async Task<DateTimeOffsetCursorPagerResult<CommentDto>> GetCommentsForPostAsViewer(Guid postId, Guid? viewerProfileId, DateTimeOffset before, int limit)
  {
    var comments = await _postCommentRepository.GetDbSet()
      .OrderByDescending(c => c.CreatedAt)
      .Where(c => c.PostId == postId && c.CreatedAt < before)
      .Include(c => c.Profile)
      .Take(limit)
      .ToListAsync();

    var commentDtos = new List<CommentDto>();

    foreach (var comment in comments)
    {
      var creator = new CommentCreatorDto
      {
        Id = comment.Profile?.ProfileId,
        Name = comment.Profile?.Name ?? "Unknown User",
        AvatarUrl = ""
      };

      var statistic = await GetCommentStatistic(comment.Id);

      var viewerReaction = viewerProfileId != null
          ? ((await _reactionRepository.GetReactionByProfileId((Guid)viewerProfileId, ReactionTargetType.Comment, comment.Id))?.ReactionType ?? ReactionType.Unknown)
          : ReactionType.Unknown;

      var commentDto = new CommentDto
      {
        Id = comment.Id,
        Content = comment.Content,
        CreatedAt = comment.CreatedAt,
        Creator = new CommentCreatorDto
        {
          Id = comment.Profile?.ProfileId,
          Name = comment.Profile?.Name ?? "Unknown User",
          AvatarUrl = ""
        },
        Statistic = statistic,
        Reaction = viewerReaction
      };

      commentDtos.Add(commentDto);
    }

    return new DateTimeOffsetCursorPagerResult<CommentDto>
    {
      Items = commentDtos,
      Next = comments.Count > 0 ? comments.Last().CreatedAt : null
    };
  }

  public async Task<CommentStatisticDto> GetCommentStatistic(Guid id)
  {
    return new CommentStatisticDto
    {
      Reactions = await _reactionRepository.CountByCommentId(id),
      Replies = 0
    };
  }
}
