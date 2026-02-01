using Fakebook.Enums;
using Fakebook.Features.CommentFeature;
using Fakebook.Features.PostFeature;
using Fakebook.Helpers;
using Fakebook.Models;
using Fakebook.Models.Entities;
using Fakebook.Models.DTOs;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fakebook.Configurations;
using Microsoft.Extensions.Options;

namespace Fakebook.Controllers;

[ApiController]
[Route("api/v1/fakebook/posts", Name = "FakebookPost")]
[Authorize]
public class FakebookPostController(
  ILogger<FakebookPostController> _logger,
  CookiesManager _cookieManager,
  IOptions<StorageConfiguration> storageConfiguration,
  AppDbContext _dbContext,
  IPostService _postService,
  IProfileService _profileService,
  ICommentService _commentService,
  IPostRepository _postRepository,
  IFileUploadService _fileUploadService,
  IPostMediaRepository _postMediaRepository,
  IPostCommentRepository _postCommentRepository,
  IReactionRepository _reactionRepository
) : FakebookController(_cookieManager, _profileService)
{
  private readonly StorageConfiguration _storageConfiguration = storageConfiguration.Value;

  #region post api

  /// <summary>
  /// Fetch feed posts
  /// </summary>
  /// <param name="request"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpGet(Name = "GetPosts")]
  public async Task<IActionResult> GetPosts([FromQuery] ListPostFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var posts = (await _postService.GetFeedByProfileAsync(profile.ProfileId, request.Before, request.Limit)).ToList();
    var postIds = posts.Select(p => p.Id).ToList();
    var postDtos = await BuildManyPostDto(posts);
    return OkResponse(postDtos);
  }

  /// <summary>
  /// Create a new post
  /// </summary>
  /// <param name="request"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpPost(Name = "CreatePost")]
  public async Task<IActionResult> CreatePost([FromBody] CreatePostFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    await using var transaction = await _dbContext.Database.BeginTransactionAsync();

    try
    {
      var post = new Post()
      {
        Id = Guid.NewGuid(),
        Content = request.Content ?? "",
        Visibility = request.Visibility,
        CreatedBy = profile.ProfileId
      };
      await _dbContext.AddAsync(post);
      await _dbContext.SaveChangesAsync();

      foreach (var mediaItem in request.MediaItems)
      {
        var postMedia = new PostMedia()
        {
          PostMediaId = Guid.NewGuid(),
          PostId = post.Id,
          Name = mediaItem.Name,
          ContentType = mediaItem.ContentType,
          FileType = mediaItem.ContentType.StartsWith("image/")
            ? MediaType.Image
            : mediaItem.ContentType.StartsWith("video/")
              ? MediaType.Video
              : mediaItem.ContentType.StartsWith("audio/")
                ? MediaType.Audio
                : MediaType.Unknown,
          Path = mediaItem.Path,
          Size = mediaItem.Size,
        };
        await _dbContext.AddAsync(postMedia);
        await _dbContext.SaveChangesAsync();
      }
      await transaction.CommitAsync();

      return OkResponse(new CreatePostFeatureResponse() { Id = post.Id });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Create post failed");
      await transaction.RollbackAsync();
      return ErrorResponse(["Failed to create new post"]);
    }
  }

  /// <summary>
  /// Get post by id
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpGet("{id}", Name = "GetPost")]
  public async Task<IActionResult> GetPost(Guid id)
  {
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    var postDto = await BuildManyPostDto([post]);
    return OkResponse(postDto.First());
  }

  /// <summary>
  /// Update post content
  /// </summary>
  /// <param name="id"></param>
  /// <param name="request"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpPatch("{id}/update-content", Name = "UpdatePostContent")]
  public async Task<IActionResult> UpdatePostContent(Guid id, [FromBody] UpdatePostFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    if (post.CreatedBy != profile.ProfileId) throw new Exception("Action not allowed");
    if (request.Content == null || request.Content.Trim().Length == 0) throw new Exception("Content is required");

    post.Content = request.Content;
    _dbContext.Posts.Update(post);
    await _dbContext.SaveChangesAsync();
    return OkResponse(new { Id = id, post.Content });
  }

  /// <summary>
  /// Delete a post
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpDelete("{id}", Name = "DeletePost")]
  public async Task<IActionResult> DeletePost(Guid id)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    if (post.CreatedBy != profile.ProfileId)
    {
      throw new Exception("Action not allowed");
    }
    _dbContext.Posts.Remove(post);
    await _dbContext.SaveChangesAsync();

    return OkResponse(new { Id = id });
  }

  /// <summary>
  /// Reaction on a post
  /// </summary>
  /// <param name="id"></param>
  /// <param name="request"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpPost("{id}/react", Name = "ReactPost")]
  public async Task<IActionResult> ReactPost(Guid id, [FromBody] ReactPostFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    var existReaction = await _dbContext.Reactions
      .Where(e => e.TargetType == ReactionTargetType.Post
        && e.TargetId == post.Id
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
        TargetType = ReactionTargetType.Post,
        TargetId = post.Id,
        ReactedBy = profile.ProfileId,
        ReactionType = request.Type,
      };
      await _dbContext.Reactions.AddAsync(newReaction);
      await _dbContext.SaveChangesAsync();
    }

    return OkResponse(new ReactPostFeatureResponse() { Id = id });
  }

  /// <summary>
  /// Share a post
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  /// <exception cref="NotImplementedException"></exception>
  [HttpPost("{id}/share", Name = "SharePost")]
  public async Task<IActionResult> SharePost(Guid id)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Get post statistic, including like count, comment count, share count
  /// </summary>
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("{id}/statistic", Name = "PostStatistic")]
  public async Task<IActionResult> Statistic(Guid id)
  {
    var statisticDto = await _postService.GetPostStatistic(id);
    return OkResponse(statisticDto);
  }

  #endregion

  #region comment api

  // TODO: Need to refactor comment APIs
  /// <summary>
  /// Get comments for a post
  /// </summary>
  /// <param name="id"></param>
  /// <param name="request"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpGet("{id}/comments", Name = "GetComments")]
  public async Task<IActionResult> GetComments(Guid id, [FromQuery] ListCommentsFeatureRequest request)
  {
    var profile = await GetActiveProfile();
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    var comments = await _commentService.GetCommentsForPostAsViewer(id, profile != null ? profile.ProfileId : null, request.Before, request.Limit);
    return OkResponse(comments);
  }

  /// <summary>
  /// Create a new comment
  /// </summary>
  /// <param name="id"></param>
  /// <param name="request"></param>
  /// <returns></returns>
  /// <exception cref="Exception"></exception>
  [HttpPost("{id}/comments", Name = "CreateComment")]
  public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentFeatureRequest request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");

    var newComment = new PostComment()
    {
      Id = Guid.NewGuid(),
      PostId = id,
      Content = request.Content,
      CreatedBy = (await GetActiveProfile())?.ProfileId ?? throw new Exception("No active profile")
    };
    await _dbContext.PostComments.AddAsync(newComment);
    await _dbContext.SaveChangesAsync();

    return OkResponse(new { newComment.Id });
  }

  #endregion

  private async Task<List<PostDto>> BuildManyPostDto(List<Post> posts)
  {
    List<PostDto> postDtos = [];
    var activeProfile = await GetActiveProfile();
    var postIds = posts.Select(e => e.Id).Distinct().ToList();
    var profileIds = posts.Select(e => e.CreatedBy).Distinct().ToList();
    var profiles = await _dbContext.Profiles
      .Where(e => profileIds.Contains(e.ProfileId))
      .ToListAsync();
    var mediaItems = await _dbContext.PostMedia
      .Where(e => postIds.Contains(e.PostId))
      .ToListAsync();

    var mediaItemPaths = mediaItems.Select(e => e.Path).Distinct().ToList();
    var mediaItemUrls = new Dictionary<string, string>();
    foreach (var path in mediaItemPaths)
    {
      var url = await _fileUploadService.GenerateFileUrl(path);
      mediaItemUrls[path] = url;
    }

    var avatarPaths = profiles
      .Where(e => e.Avatar != null)
      .Select(e => e.Avatar!)
      .Distinct()
      .ToList();
    var avatarUrls = new Dictionary<string, string>();
    foreach (var path in avatarPaths)
    {
      var url = await _fileUploadService.GenerateFileUrl(path);
      avatarUrls[path] = url;
    }

    foreach (var post in posts)
    {
      var creatorProfile = profiles.FirstOrDefault(e => e.ProfileId == post.CreatedBy);
      var extractMediaItems = mediaItems.Where(e => e.PostId == post.Id).ToList();
      List<MediaItemDto> mediaItemDtos = [.. extractMediaItems.Select(e =>
      {
        return new MediaItemDto
        {
          Id = e.PostMediaId,
          Name = e.Name,
          Path = e.Path,
          Source = mediaItemUrls[e.Path],
          Type = e.FileType switch
          {
            MediaType.Image => "image",
            MediaType.Video => "video",
            MediaType.Audio => "audio",
            _ => "unknown"
          }
        };
      })];

      var reaction = activeProfile == null ? null : await _reactionRepository.GetReactionByProfileId(
        activeProfile.ProfileId,
        ReactionTargetType.Post,
        post.Id
      );

      var postDto = new PostDto()
      {
        Id = post.Id,
        Content = post.Content,
        CreatedAt = post.CreatedAt,
        Creator = new PostCreatorDto
        {
          Id = post.CreatedBy,
          Name = creatorProfile?.Name ?? "Unknown user",
          AvatarUrl = creatorProfile?.Avatar == null ? null : avatarUrls[creatorProfile.Avatar]
        },
        MediaItems = [.. mediaItemDtos],
        Statistic = await _postService.GetPostStatistic(post.Id),
        Reaction = reaction == null ? ReactionType.Unknown : reaction.ReactionType
      };

      postDtos.Add(postDto);
    }

    return postDtos;
  }
}
