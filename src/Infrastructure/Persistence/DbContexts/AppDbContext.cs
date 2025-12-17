using Microsoft.EntityFrameworkCore;
using YumiStudio.Domain.Entities;
using FakebookEntities = YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Infrastructure.Persistence.Configurations;

namespace YumiStudio.Infrastructure.Persistence.DbContexts;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Module base
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new FileUploadConfiguration());

    // Module Fakebook
    modelBuilder.ApplyConfiguration(new FakebookConfiguration.ProfileConfiguration());
    modelBuilder.ApplyConfiguration(new FakebookConfiguration.PostConfiguration());
    modelBuilder.ApplyConfiguration(new FakebookConfiguration.CommentConfiguration());
    modelBuilder.ApplyConfiguration(new FakebookConfiguration.MediaConfiguration());
    modelBuilder.ApplyConfiguration(new FakebookConfiguration.ReactionConfiguration());
  }

  #region Basic Deset

  public DbSet<User> Users { get; set; }
  public DbSet<FileUpload> FileUploads { get; set; }

  #endregion

  #region Fakebook Dbset

  public DbSet<FakebookEntities.Profile> FakebookProfiles { get; set; }
  public DbSet<FakebookEntities.Post> FakebookPosts { get; set; }
  public DbSet<FakebookEntities.PostMedia> FakebookPostMedias { get; set; }
  public DbSet<FakebookEntities.PostComment> FakebookPostComments { get; set; }
  public DbSet<FakebookEntities.Reaction> FakebookReactions { get; set; }

  #endregion
}
