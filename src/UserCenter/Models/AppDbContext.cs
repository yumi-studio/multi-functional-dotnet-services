using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UserApi.Models;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<User>()
      .Property(e => e.BirthDate)
      .HasConversion(new ValueConverter<DateOnly, DateTime>(
        d => d.ToDateTime(TimeOnly.MinValue),
        d => DateOnly.FromDateTime(d)
      ))
      .HasColumnType("date");
    modelBuilder.Entity<User>().HasMany<Token>().WithOne(e => e.User).HasForeignKey(e => e.UserId);
    modelBuilder.Entity<User>().HasMany<UserExternal>().WithOne(e => e.User).HasForeignKey(e => e.UserId);
  }

  public DbSet<User> Users { get; set; }
  public DbSet<UserExternal> UserExternals { get; set; }
}
