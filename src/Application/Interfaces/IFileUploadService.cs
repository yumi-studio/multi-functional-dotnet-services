using System;
using YumiStudio.Application.Services;
using YumiStudio.Domain.Entities;

namespace YumiStudio.Application.Interfaces;

public interface IFileUploadService
{
  public Task<FileUpload> Upload(IFormFile file, FileCustomOption? option = null);

  public Task<string?> GetFileUrl(string filePath);
}
