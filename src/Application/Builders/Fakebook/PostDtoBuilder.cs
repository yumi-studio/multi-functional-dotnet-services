using System;
using YumiStudio.Application.DTOs.Fakebook;
using YumiStudio.Application.Interfaces.Fakebook;
using YumiStudio.Common.Helpers;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;

namespace YumiStudio.Application.Builders.Fakebook;

public class PostDtoBuilder()
{
  private Post? Post { get; set; }
  private ProfileDto? Creator { get; set; }
  private Reaction? Reaction { get; set; }
  private PostStatisticDto? Statistic { get; set; }

  public void Clear()
  {
    Post = null;
    Creator = null;
    Reaction = null;
    Statistic = null;
  }

  public PostDtoBuilder WithPost(Post post)
  {
    Post = post;
    return this;
  }

  public PostDtoBuilder WithCreator(ProfileDto creator)
  {
    Creator = creator;
    return this;
  }

  public PostDtoBuilder WithStatistic(PostStatisticDto statisticDto)
  {
    Statistic = statisticDto;
    return this;
  }

  public PostDto CreateFeedPost()
  {
    if (Post == null) throw new Exception("Post is required");

    var postDto = new PostDto
    {
      Content = Post.Content,
      CreatedAt = Post.CreatedAt,
      MediaItems = [],
      Visibility = Post.Visibility,
      Creator = new PostCreatorDto
      {
        Id = Creator.Id
      }
    };

    Clear();

    return postDto;
  }
}
