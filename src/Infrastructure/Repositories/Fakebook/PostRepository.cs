using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories.Fakebook;

public class PostRepository(AppDbContext dbContext) : BaseRepository<Post, Guid>(dbContext), IPostRepository
{
  public async Task<Post?> GetByIdAsync(Guid id, params Expression<Func<Post, object>>[] includes)
  {
    IQueryable<Post> query = _dbSet;
    foreach (var include in includes)
    {
      query = query.Include(include);
    }

    var post = await query.Where(e => e.Id == id).FirstOrDefaultAsync();

    return post;
  }

  public async Task<Post?> GetByIdWithReferencesAsync(Guid id)
  {
    return await GetByIdAsync(id, [
      e => e.Profile
    ]);
  }

  public async Task<IEnumerable<Post>> GetPostsAsync(DateTimeOffset? createdAfter, DateTimeOffset? createdBefore, int limit)
  {
    var before = createdBefore ?? DateTimeOffset.MaxValue;
    var after = createdAfter ?? DateTimeOffset.MinValue;

    return await _dbSet
        .Where(p => p.CreatedAt >= after && p.CreatedAt <= before)
        .OrderByDescending(p => p.CreatedAt)
        .Take(limit)
        .ToListAsync();
  }
}
