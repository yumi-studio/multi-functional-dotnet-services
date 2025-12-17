using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Application.Features.Fakebook;
using YumiStudio.Application.Interfaces.Fakebook;
using YumiStudio.Common.Helpers;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Enums.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.API.Controllers.Fakebook;

[ApiController]
[Route("api/v1/fakebook/comments", Name = "FakebookComment")]
[Authorize]
public class FakebookCommentController(
  ILogger<FakebookCommentController> _logger,
  CookiesManager _cookieManager,
  AppDbContext _dbContext,
  IProfileService _profileService,
  ICommentService _commentService,
  IPostRepository _postRepository,
  IPostCommentRepository _postCommentRepository,
  IReactionRepository _reactionRepository,
  FakebookHelper _fakebookHelper
) : FakebookController(_cookieManager, _profileService)
{
  [HttpGet("{id}", Name = "GetComment")]
  public async Task<IActionResult> GetComment(Guid id)
  {
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");
    return OkResponse(await BuildCommentDto(comment));
  }

  [HttpPut("{id}", Name = "UpdateComment")]
  public async Task<IActionResult> UpdateComment(Guid id, [FromBody] CommentRequest.UpdateComment request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");
    if (comment.CreatedBy != profile.ProfileId)
    {
      throw new Exception("Action not allowed");
    }
    comment.Content = request.Content;
    _dbContext.FakebookPostComments.Update(comment);
    await _dbContext.SaveChangesAsync();
    return OkResponse(await BuildCommentDto(comment));
  }

  [HttpDelete("{id}", Name = "DeleteComment")]
  public async Task<IActionResult> DeleteComment(Guid id)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");
    if (comment.CreatedBy != profile.ProfileId)
    {
      throw new Exception("Action not allowed");
    }
    _dbContext.FakebookPostComments.Remove(comment);
    await _dbContext.SaveChangesAsync();
    return OkResponse<object?>(null);
  }

  [HttpPost("{id}/react", Name = "ReactComment")]
  public async Task<IActionResult> ReactComment(Guid id, [FromBody] CommentRequest.ReactComment request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");

    var existReaction = await _dbContext.FakebookReactions
      .Where(e => e.TargetType == ReactionTargetType.Comment
        && e.TargetId == comment.Id
        && e.ReactedBy == profile.ProfileId
      )
      .FirstOrDefaultAsync();

    if (existReaction != null)
    {
      if (existReaction.ReactionType == request.Type)
      {
        _dbContext.FakebookReactions.Remove(existReaction);
        await _dbContext.SaveChangesAsync();
      }
      else
      {
        existReaction.ReactionType = request.Type;
        _dbContext.FakebookReactions.Update(existReaction);
        await _dbContext.SaveChangesAsync();
      }
    }
    else
    {
      var newReaction = new Reaction
      {
        Id = Guid.NewGuid(),
        TargetType = ReactionTargetType.Comment,
        TargetId = comment.Id,
        ReactedBy = profile.ProfileId,
        ReactionType = request.Type,
      };
      await _dbContext.FakebookReactions.AddAsync(newReaction);
      await _dbContext.SaveChangesAsync();
    }

    return OkResponse<object?>(null);
  }

  [HttpGet("{id}/statistic", Name = "GetStatistic")]
  public async Task<IActionResult> GetStatistic(Guid id)
  {
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");
    return OkResponse(await _commentService.GetCommentStatistic(id));
  }

  private async Task<CommentDto> BuildCommentDto(PostComment comment)
  {
    await _dbContext.FakebookPostComments.Entry(comment).Reference(e => e.Profile).LoadAsync();

    return new CommentDto
    {
      Id = comment.Id,
      Content = comment.Content,
      CreatedAt = comment.CreatedAt,
      UpdatedAt = comment.UpdatedAt,
      Creator = new CommentCreatorDto
      {
        Id = comment.Profile.ProfileId,
        Name = comment.Profile.Name,
        AvatarUrl = await _fakebookHelper.GetProfileAvatar()
      },
      Statistic = await _commentService.GetCommentStatistic(comment.Id)
    };
  }
}
