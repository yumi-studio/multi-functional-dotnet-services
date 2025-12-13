using System;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

namespace YumiStudio.YumiDotNet.Domain.Interfaces.Fakebook;

public interface IPostCommentRepository : IRepository<PostComment, Guid>
{
  public Task<int> CountByPostId(Guid postId);
}
