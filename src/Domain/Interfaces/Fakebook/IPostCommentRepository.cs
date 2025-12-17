using System;
using YumiStudio.Domain.Entities.Fakebook;

namespace YumiStudio.Domain.Interfaces.Fakebook;

public interface IPostCommentRepository : IRepository<PostComment, Guid>
{
  public Task<int> CountByPostId(Guid postId);
}
