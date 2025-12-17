using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YumiStudio.Domain.Entities;
using YumiStudio.Domain.Entities.Fakebook;

namespace YumiStudio.Infrastructure.Persistence.Configurations;

public class FakebookConfiguration
{
  public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
  {
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
      builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
      builder.HasMany<Post>().WithOne(e => e.Profile).HasForeignKey(e => e.CreatedBy);
      builder.HasMany<PostComment>().WithOne(e => e.Profile).HasForeignKey(e => e.CreatedBy);
      builder.HasMany<Reaction>().WithOne(e => e.Profile).HasForeignKey(e => e.ReactedBy);
    }
  }
  public class PostConfiguration : IEntityTypeConfiguration<Post>
  {
    public void Configure(EntityTypeBuilder<Post> builder)
    {
      builder.HasOne(e => e.Profile).WithMany().HasForeignKey(e => e.CreatedBy);
      builder.HasMany<PostMedia>().WithOne(e => e.Post).HasForeignKey(e => e.PostId);
      builder.HasMany<PostComment>().WithOne(e => e.Post).HasForeignKey(e => e.PostId);
    }
  }

  public class MediaConfiguration : IEntityTypeConfiguration<PostMedia>
  {
    public void Configure(EntityTypeBuilder<PostMedia> builder)
    {
      builder.HasOne(e => e.FileUpload)
        .WithOne(e => e.MediaItem)
        .HasForeignKey<PostMedia>(e => e.FileId)
        .IsRequired();
    }
  }

  public class CommentConfiguration : IEntityTypeConfiguration<PostComment>
  {
    public void Configure(EntityTypeBuilder<PostComment> builder)
    {
      builder.HasOne(e => e.Profile).WithMany().HasForeignKey(e => e.CreatedBy);
      builder.HasOne(e => e.Post).WithMany().HasForeignKey(e => e.PostId);
    }
  }

  public class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
  {
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
    }
  }
}
