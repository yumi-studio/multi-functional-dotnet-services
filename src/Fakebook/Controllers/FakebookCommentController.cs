using System;
using Fakebook.Enums;
using Fakebook.Features.CommentFeature;
using Fakebook.Helpers;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Controllers;

[ApiController]
[Route("api/v1/fakebook/comments", Name = "FakebookComment")]
[Authorize]
public class FakebookCommentController(
  ILogger<FakebookCommentController> _logger,
  CookiesManager _cookieManager,
  AppDbContext _dbContext,
  IProfileService _profileService,
  ICommentService _commentService,
  IPostCommentRepository _postCommentRepository
) : FakebookController(_cookieManager, _profileService)
{
  [HttpGet("{id}", Name = "GetComment")]
  public async Task<IActionResult> GetComment(Guid id)
  {
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");
    return OkResponse(await BuildCommentDto(comment));
  }

  [HttpPut("{id}", Name = "UpdateComment")]
  public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");
    if (comment.CreatedBy != profile.ProfileId)
    {
      throw new Exception("Action not allowed");
    }
    comment.Content = request.Content;
    _dbContext.PostComments.Update(comment);
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
    _dbContext.PostComments.Remove(comment);
    await _dbContext.SaveChangesAsync();
    return OkResponse();
  }

  [HttpPost("{id}/react", Name = "ReactComment")]
  public async Task<IActionResult> ReactComment(Guid id, [FromBody] ReactCommentFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var comment = await _postCommentRepository.GetByIdAsync(id) ?? throw new Exception("Comment is not exist");

    var existReaction = await _dbContext.Reactions
      .Where(e => e.TargetType == ReactionTargetType.Comment
        && e.TargetId == comment.Id
        && e.ReactedBy == profile.ProfileId
      )
      .FirstOrDefaultAsync();

    if (existReaction != null)
    {
      if (existReaction.ReactionType == request.Type)
      {
        _dbContext.Reactions.Remove(existReaction);
        await _dbContext.SaveChangesAsync();
      }
      else
      {
        existReaction.ReactionType = request.Type;
        _dbContext.Reactions.Update(existReaction);
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
      await _dbContext.Reactions.AddAsync(newReaction);
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
    await _dbContext.PostComments.Entry(comment).Reference(e => e.Profile).LoadAsync();

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
        AvatarUrl = null
      },
      Statistic = await _commentService.GetCommentStatistic(comment.Id)
    };
  }
}
