using System;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Services.Interfaces;

public interface ICommentService
{
  public Task<CommentStatisticDto> GetCommentStatistic(Guid id);

  public Task<DateTimeOffsetCursorPagerResult<CommentDto>> GetCommentsForPostAsViewer(
    Guid postId,
    Guid? viewerProfileId,
    DateTimeOffset before,
    int limit
  );
}
