using System;
using Microsoft.Extensions.Options;
using Fakebook.Configurations;
using Fakebook.Services.Interfaces;
using Fakebook.Infrastructure.Resolvers;
using Fakebook.Enums;
using Fakebook.Infrastructure.UploadMethods;

namespace Fakebook.Services;

public class FileUploadService(
  IUploadMethodResolver _uploadMethodResolver
) : IFileUploadService
{
  private readonly IUploadMethod Uploader = _uploadMethodResolver.Resolve(UploadMethod.Local);

  public async Task<bool> DeleteFile(string path)
  {
    return await Uploader.DeleteAsync(path);
  }

  public async Task<string> GenerateFileUrl(string path)
  {
    return await Uploader.GetUrlAsync(path);
  }

  public async Task<UploadedFile> UploadFile(Stream fileStream, UploadOptions options)
  {
    var fileName = options.FileName;
    var directory = options.Directory;
    var contentType = options.ContentType;
    var path = await Uploader.UploadAsync(fileStream, fileName, directory);

    // Save file 
    var uploadedFile = new UploadedFile
    {
      Name = fileName,
      Path = path,
      ContentType = contentType,
      Size = fileStream.Length
    };
    return uploadedFile;
  }
}
