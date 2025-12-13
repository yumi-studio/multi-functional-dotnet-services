using System;
using YumiStudio.YumiDotNet.Application.Services;
using YumiStudio.YumiDotNet.Domain.Entities;

namespace YumiStudio.YumiDotNet.Application.Interfaces;

public interface IFileUploadService
{
  public Task<FileUpload> Upload(IFormFile file, FileCustomOption? option = null);

  public Task<string?> GetFileUrl(string filePath);
}
