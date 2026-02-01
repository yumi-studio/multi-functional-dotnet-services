using System;
using System.Linq.Expressions;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Repositories.Interfaces;

public interface IPostRepository : IRepository<Post, Guid>
{
  public Task<Post?> GetByIdAsync(Guid id, params Expression<Func<Post, object>>[] includes);
  public Task<Post?> GetByIdWithReferencesAsync(Guid id);
  public Task<IEnumerable<Post>> GetPostsAsync(
    DateTimeOffset? createdAfter,
    DateTimeOffset? createdBefore,
    int limit
  );
}
