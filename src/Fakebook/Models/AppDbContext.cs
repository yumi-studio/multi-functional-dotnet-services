using System;
using Microsoft.EntityFrameworkCore;

namespace Fakebook.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Profile relationship
    var profile = modelBuilder.Entity<Entities.Profile>();
    profile.HasMany<Entities.Post>().WithOne(e => e.Profile).HasForeignKey(e => e.CreatedBy);
    profile.HasMany<Entities.PostComment>().WithOne(e => e.Profile).HasForeignKey(e => e.CreatedBy);
    profile.HasMany<Entities.Reaction>().WithOne(e => e.Profile).HasForeignKey(e => e.ReactedBy);

    var post = modelBuilder.Entity<Entities.Post>();
    post.HasOne(e => e.Profile).WithMany().HasForeignKey(e => e.CreatedBy);
    post.HasMany<Entities.PostMedia>().WithOne(e => e.Post).HasForeignKey(e => e.PostId);
    post.HasMany<Entities.PostComment>().WithOne(e => e.Post).HasForeignKey(e => e.PostId);

    var postComment = modelBuilder.Entity<Entities.PostComment>();
    postComment.HasOne(e => e.Profile).WithMany().HasForeignKey(e => e.CreatedBy);
    postComment.HasOne(e => e.Post).WithMany().HasForeignKey(e => e.PostId);
  }

  public DbSet<Entities.Profile> Profiles { get; set; }
  public DbSet<Entities.Post> Posts { get; set; }
  public DbSet<Entities.PostMedia> PostMedia { get; set; }
  public DbSet<Entities.PostComment> PostComments { get; set; }
  public DbSet<Entities.Reaction> Reactions { get; set; }
}
