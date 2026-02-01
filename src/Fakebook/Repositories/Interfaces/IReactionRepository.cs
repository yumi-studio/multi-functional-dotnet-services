using System;
using Fakebook.Enums;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;

namespace Fakebook.Repositories.Interfaces;

public interface IReactionRepository : IRepository<Reaction, Guid>
{
  public Task<ReactionDto> CountByPostId(Guid postId);
  public Task<ReactionDto> CountByCommentId(Guid postId);
  public Task<Reaction?> GetReactionByProfileId(Guid profileId, ReactionTargetType targetType, Guid targetId);
  public Task<IEnumerable<Reaction>> GetReactionsByProfileId(Guid profileId, ReactionTargetType targetType, IEnumerable<Guid> targetIds);
}
