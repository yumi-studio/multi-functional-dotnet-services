using System;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Repositories.Interfaces;

public interface IPostCommentRepository : IRepository<PostComment, Guid>
{
  public Task<int> CountByPostId(Guid postId);
}
