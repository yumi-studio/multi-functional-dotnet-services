using System;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Repositories;

public class PostMediaRepository(AppDbContext appDbContext) : BaseRepository<PostMedia, Guid>(appDbContext), IPostMediaRepository
{
  public async Task<List<PostMedia>> GetListByPostId(Guid PostId)
  {
    return await _dbSet.Where(e => e.PostId == PostId).ToListAsync();
  }
}
