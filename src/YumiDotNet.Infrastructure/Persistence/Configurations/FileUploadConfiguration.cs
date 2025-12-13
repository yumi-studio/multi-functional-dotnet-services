using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YumiStudio.YumiDotNet.Domain.Entities;
using FakebookEntities = YumiStudio.YumiDotNet.Domain.Entities.Fakebook;

namespace YumiStudio.YumiDotNet.Infrastructure.Persistence.Configurations;

public class FileUploadConfiguration : IEntityTypeConfiguration<FileUpload>
{
  public void Configure(EntityTypeBuilder<FileUpload> builder)
  {
    builder.HasOne(e => e.MediaItem)
      .WithOne(e => e.FileUpload)
      .HasForeignKey<FakebookEntities.PostMedia>(e => e.FileId)
      .IsRequired();
  }
}
