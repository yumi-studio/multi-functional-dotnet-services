using System;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Domain.Entities.Fakebook;

namespace YumiStudio.Application.Interfaces.Fakebook;

public interface IPostService
{
  public Task<IEnumerable<Post>> GetFeedAsync(DateTimeOffset? before, int limit);

  public Task<PostStatisticDto> GetPostStatistic(Guid id);
}
