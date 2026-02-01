using System;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Repositories;

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
