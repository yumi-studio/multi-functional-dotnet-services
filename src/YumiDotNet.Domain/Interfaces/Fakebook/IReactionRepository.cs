using System;
using YumiStudio.YumiDotNet.Application.DTOs.Fakebook;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;
using YumiStudio.YumiDotNet.Domain.Enums.Fakebook;

namespace YumiStudio.YumiDotNet.Domain.Interfaces.Fakebook;

public interface IReactionRepository : IRepository<Reaction, Guid>
{
  public Task<ReactionDto> CountByPostId(Guid postId);
  public Task<ReactionDto> CountByCommentId(Guid postId);
  public Task<Reaction?> GetReactionByProfileId(Guid profileId, ReactionTargetType targetType, Guid targetId);
  public Task<IEnumerable<Reaction>> GetReactionsByProfileId(Guid profileId, ReactionTargetType targetType, IEnumerable<Guid> targetIds);
}
