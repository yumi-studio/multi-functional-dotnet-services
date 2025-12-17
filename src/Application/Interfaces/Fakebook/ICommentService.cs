using System;
using YumiStudio.Application.DTOs;
using YumiStudio.Application.DTOs.Fakebook;

namespace YumiStudio.Application.Interfaces.Fakebook;

public interface ICommentService
{
  public Task<CommentStatisticDto> GetCommentStatistic(Guid id);

  public Task<DateTimeOffsetCursorPagerResult<CommentDto>> GetCommentsForPostAsViewer(Guid postId, Guid? viewerProfileId, DateTimeOffset before, int limit);
}
