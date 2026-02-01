using System;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;

namespace Fakebook.Repositories;

public class PostMediaRepository(AppDbContext appDbContext) : BaseRepository<PostMedia, Guid>(appDbContext), IPostMediaRepository
{
  

}
