using System;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Repositories.Interfaces;

public interface IPostMediaRepository : IRepository<PostMedia, Guid>
{
  public Task<List<PostMedia>> GetListByPostId(Guid PostId);
}
