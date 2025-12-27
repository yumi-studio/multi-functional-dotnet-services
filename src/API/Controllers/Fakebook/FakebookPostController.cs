using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;
using YumiStudio.Domain.Enums.Fakebook;
using YumiStudio.Application.Interfaces;
using YumiStudio.Domain.Interfaces;
using YumiStudio.Application.Services;
using YumiStudio.Application.Features.Fakebook;
using YumiStudio.Infrastructure.Persistence.DbContexts;
using YumiStudio.Application.Interfaces.Fakebook;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Common.Helpers;

namespace YumiStudio.API.Controllers.Fakebook;

[ApiController]
[Route("api/v1/fakebook/posts", Name = "FakebookPost")]
[Authorize]
public class FakebookPostController(
  ILogger<FakebookPostController> _logger,
  CookiesManager _cookieManager,
  FakebookHelper _fakebookHelper,
  AppDbContext _dbContext,
  IFileUploadService _fileUploadService,
  IPostService _postService,
  IProfileService _profileService,
  ICommentService _commentService,
  IPostRepository _postRepository,
  // IPostMediaRepository _postMediaRepository,
  // IPostCommentRepository _postCommentRepository,
  IFileUploadRepository _fileUploadRepository,
  IReactionRepository _reactionRepository
) : FakebookController(_cookieManager, _profileService)
{
  private readonly string _uploadDir = "uploads/fakebook/";

  #region post api

  [HttpGet(Name = "GetPosts")]
  public async Task<IActionResult> GetPosts([FromQuery] PostRequest.GetPosts request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var posts = await _postService.GetFeedAsync(request.Before, request.Limit);
    var postIds = posts.Select(p => p.Id).ToArray();
    var postDtos = await BuildManyPostDto(posts);
    return OkResponse(postDtos);
  }

  [HttpPost("upload-media", Name = "UploadMedia")]
  public async Task<IActionResult> UploadMedia([FromForm] PostRequest.UploadMedia request)
  {
    // TODO: Need to refactor, so that everything is reverted if anything errors
    var file = request.MediaItem;
    var fileCustomOption = new FileCustomOption { SaveDirPath = _uploadDir, UploaderId = GetAuthenticatedUserId() };
    var fileUpload = await _fileUploadService.Upload(file, fileCustomOption);

    return OkResponse(new PostResponse.UploadMedia
    {
      FileUploadId = fileUpload.Id
    });
  }

  [HttpPost(Name = "CreatePost")]
  public async Task<IActionResult> CreatePost([FromBody] PostRequest.CreatePost request)
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

      var foundFiles = await _fileUploadRepository.GetDbSet()
        .Where(fu => request.MediaItemIds!.Contains(fu.Id)).ToListAsync();

      foreach (var ff in foundFiles)
      {
        var postMedia = new PostMedia() { PostMediaId = Guid.NewGuid(), FileId = ff.Id, PostId = post.Id };
        await _dbContext.AddAsync(postMedia);
        await _dbContext.SaveChangesAsync();
      }
      await transaction.CommitAsync();

      return OkResponse(new PostResponse.CreatePost() { PostId = post.Id });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Create post failed");
      await transaction.RollbackAsync();
      return ErrorResponse(["Failed to create new post"]);
    }
  }

  [HttpGet("{id}", Name = "GetPost")]
  public async Task<IActionResult> GetPost(Guid id)
  {
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    var postDto = await BuildManyPostDto([post]);
    return OkResponse(postDto.First());
  }

  [HttpPatch("{id}/update-content", Name = "UpdatePostContent")]
  public async Task<IActionResult> UpdatePostContent(Guid id, [FromBody] PostRequest.UpdatePostContent request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    if (post.CreatedBy != profile.ProfileId) throw new Exception("Action not allowed");
    if (request.Content == null || request.Content.Trim().Length == 0) throw new Exception("Content is required");

    post.Content = request.Content;
    _dbContext.FakebookPosts.Update(post);
    await _dbContext.SaveChangesAsync();
    return OkResponse(new { Id = id, post.Content });
  }

  [HttpDelete("{id}", Name = "DeletePost")]
  public async Task<IActionResult> DeletePost(Guid id)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    if (post.CreatedBy != profile.ProfileId)
    {
      throw new Exception("Action not allowed");
    }
    _dbContext.FakebookPosts.Remove(post);
    await _dbContext.SaveChangesAsync();

    return OkResponse(new { Id = id });
  }

  [HttpPost("{id}/react", Name = "ReactPost")]
  public async Task<IActionResult> ReactPost(Guid id, [FromBody] PostRequest.ReactPost request)
  {
    var profile = await GetActiveProfile() ?? throw new Exception("No active profile");
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    var existReaction = await _dbContext.FakebookReactions
      .Where(e => e.TargetType == ReactionTargetType.Post
        && e.TargetId == post.Id
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
        TargetType = ReactionTargetType.Post,
        TargetId = post.Id,
        ReactedBy = profile.ProfileId,
        ReactionType = request.Type,
      };
      await _dbContext.FakebookReactions.AddAsync(newReaction);
      await _dbContext.SaveChangesAsync();
    }

    return OkResponse(new { Id = id });
  }

  // [HttpPost("{id}/share", Name = "SharePost")]
  // public async Task<IActionResult> SharePost(Guid id, [FromBody] PostRequest.SharePost request)
  // {
  //   throw new NotImplementedException();
  // }

  [HttpGet("{id}/statistic", Name = "PostStatistic")]
  public async Task<IActionResult> Statistic(Guid id)
  {
    var statisticDto = await _postService.GetPostStatistic(id);
    return OkResponse(statisticDto);
  }

  #endregion

  #region comment api
  // TODO: Need to refactor comment APIs

  [HttpGet("{id}/comments", Name = "GetComments")]
  public async Task<IActionResult> GetComments(Guid id, [FromQuery] CommentRequest.GetComments request)
  {
    var profile = await GetActiveProfile();
    var post = await _postRepository.GetByIdAsync(id) ?? throw new Exception("Post is not exist");
    var comments = await _commentService.GetCommentsForPostAsViewer(id, profile != null ? profile.ProfileId : null, request.Before, request.Limit);
    return OkResponse(comments);
  }

  [HttpPost("{id}/comments", Name = "CreateComment")]
  public async Task<IActionResult> CreateComment(Guid id, [FromBody] CommentRequest.CreateComment request)
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
    await _dbContext.FakebookPostComments.AddAsync(newComment);
    await _dbContext.SaveChangesAsync();

    return OkResponse(new { newComment.Id });
  }

  #endregion

  public static MediaType GetFileCategory(IFormFile file)
  {
    // Use ContentType (MIME type)
    var contentType = file.ContentType.ToLower();

    if (contentType.StartsWith("image/"))
      return MediaType.Image;
    if (contentType.StartsWith("video/"))
      return MediaType.Video;
    if (contentType.StartsWith("audio/"))
      return MediaType.Audio;

    // Optionally, check file extension as a fallback
    var extension = Path.GetExtension(file.FileName).ToLower();
    var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
    var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };
    var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".aac" };

    if (imageExtensions.Contains(extension))
      return MediaType.Image;
    if (videoExtensions.Contains(extension))
      return MediaType.Video;
    if (audioExtensions.Contains(extension))
      return MediaType.Audio;

    return MediaType.File;
  }

  private async Task<IEnumerable<PostDto>> BuildManyPostDto(IEnumerable<Post> posts)
  {
    List<PostDto> postDtos = [];
    var activeProfile = await GetActiveProfile();
    var postIds = posts.Select(e => e.Id).Distinct().ToList();
    var profileIds = posts.Select(e => e.CreatedBy).Distinct().ToList();
    var profiles = await _dbContext.FakebookProfiles
      .Where(e => profileIds.Contains(e.ProfileId))
      .ToListAsync();
    var mediaItems = await _dbContext.FakebookPostMedias
      .Where(e => postIds.Contains(e.PostId))
      .Include(e => e.FileUpload)
      .ToListAsync();

    foreach (var post in posts)
    {
      var creatorProfile = profiles.Where(e => e.ProfileId == post.CreatedBy).FirstOrDefault();
      var extractMediaItems = mediaItems.Where(e => e.PostId == post.Id).ToList();
      var mediaItemDtos = await Task.WhenAll(
        extractMediaItems.Select(async e => new MediaItemDto
        {
          Id = e.PostMediaId,
          Name = e.FileUpload.Name,
          Path = e.FileUpload.Path,
          Source = await _fileUploadService.GetFileUrl(e.FileUpload.Path) ?? "",
          Type = e.FileUpload.FileType switch
          {
            Domain.Enums.FileUploadType.Image => "image",
            Domain.Enums.FileUploadType.Video => "video",
            Domain.Enums.FileUploadType.Audio => "audio",
            _ => "unknown"
          }
        }).ToList()
      );
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
          AvatarUrl = await _fakebookHelper.GetProfileAvatar()
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
