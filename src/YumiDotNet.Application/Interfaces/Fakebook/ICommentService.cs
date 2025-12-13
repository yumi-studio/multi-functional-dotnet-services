using System;
using YumiStudio.YumiDotNet.Application.DTOs;
using YumiStudio.YumiDotNet.Application.DTOs.Fakebook;

namespace YumiStudio.YumiDotNet.Application.Interfaces.Fakebook;

public interface ICommentService
{
  public Task<CommentStatisticDto> GetCommentStatistic(Guid id);

  public Task<DateTimeOffsetCursorPagerResult<CommentDto>> GetCommentsForPostAsViewer(Guid postId, Guid? viewerProfileId, DateTimeOffset before, int limit);
}
