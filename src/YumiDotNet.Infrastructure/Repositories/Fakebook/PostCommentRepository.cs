using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;
using YumiStudio.YumiDotNet.Domain.Interfaces.Fakebook;
using YumiStudio.YumiDotNet.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.YumiDotNet.Infrastructure.Repositories.Fakebook;

public class PostCommentRepository(
  AppDbContext dbContext
) : BaseRepository<PostComment, Guid>(dbContext), IPostCommentRepository
{
  public async Task<int> CountByPostId(Guid postId)
  {
    var countComment = await _dbSet
      .Where(e => e.PostId == postId)
      .Select(e => e.Id)
      .ToListAsync();

    return countComment.Count;
  }
}
