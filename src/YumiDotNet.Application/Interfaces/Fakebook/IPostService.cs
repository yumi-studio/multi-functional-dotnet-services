using System;
using YumiStudio.YumiDotNet.Application.DTOs.Fakebook;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

namespace YumiStudio.YumiDotNet.Application.Interfaces.Fakebook;

public interface IPostService
{
  public Task<IEnumerable<Post>> GetFeedAsync(DateTimeOffset? before, int limit);

  public Task<PostStatisticDto> GetPostStatistic(Guid id);
}
