using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YumiStudio.YumiDotNet.Domain.Entities;
using FakebookEntities = YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

namespace YumiStudio.YumiDotNet.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.HasMany<Token>().WithOne(e => e.User).HasForeignKey(e => e.UserId);
    builder.HasMany<FileUpload>().WithOne(e => e.Uploader).HasForeignKey(e => e.UploadedBy);
    builder.HasMany<FakebookEntities.Profile>().WithOne(e => e.User).HasForeignKey(e => e.UserId);
  }
}
