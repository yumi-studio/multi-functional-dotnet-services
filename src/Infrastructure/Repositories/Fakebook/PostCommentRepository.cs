using System;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories.Fakebook;

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
