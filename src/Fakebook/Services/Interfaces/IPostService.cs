using System;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Services.Interfaces;

public interface IPostService
{
  public Task<List<Post>> GetFeedAsync(DateTimeOffset? before, int limit);
  public Task<List<Post>> GetFeedByProfileAsync(Guid profileId, DateTimeOffset? before, int limit);
  public Task<PostStatisticDto> GetPostStatistic(Guid id);
}
