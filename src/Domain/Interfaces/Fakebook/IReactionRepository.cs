using System;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Enums.Fakebook;

namespace YumiStudio.Domain.Interfaces.Fakebook;

public interface IReactionRepository : IRepository<Reaction, Guid>
{
  public Task<ReactionDto> CountByPostId(Guid postId);
  public Task<ReactionDto> CountByCommentId(Guid postId);
  public Task<Reaction?> GetReactionByProfileId(Guid profileId, ReactionTargetType targetType, Guid targetId);
  public Task<IEnumerable<Reaction>> GetReactionsByProfileId(Guid profileId, ReactionTargetType targetType, IEnumerable<Guid> targetIds);
}
