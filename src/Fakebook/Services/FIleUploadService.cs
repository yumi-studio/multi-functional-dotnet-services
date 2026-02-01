using System;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Fakebook.Configurations;
using Fakebook.Services.Interfaces;
using Fakebook.Infrastructure.Resolvers;
using Fakebook.Enums;

namespace Fakebook.Services;

public class FileUploadService(
  ILogger<FileUploadService> _logger,
  IOptions<StorageConfiguration> storageConfiguration,
  IUploadMethodResolver _uploadMethodResolver
) : IFileUploadService
{
  private readonly StorageConfiguration _storageConfiguration = storageConfiguration.Value;

  public Task<string> GenerateFileUrl(string path)
  {
    return Task.FromResult($"{_storageConfiguration.BaseUrl.TrimEnd('/')}/{_storageConfiguration.BasePath.Trim('/')}/{path.Replace("\\", "/").Trim('/')}");
  }

  // public async Task<UploadedFile> UploadFile(Stream fileStream, string fileName, UploadMethod method)
  // {
  //   return await UploadFileToDirectory(fileStream, fileName, string.Empty, method);
  // }

  // public async Task<UploadedFile> UploadFileToDirectory(Stream fileStream, string fileName, string directory, UploadMethod method)
  // {
  //   var uploader = _uploadMethodResolver.Resolve(method);
  //   var path = await uploader.UploadAsync(fileStream, fileName, directory);

  //   // Save file 
  //   var uploadedFile = new UploadedFile
  //   {
  //     Name = fileName,
  //     Path = path,
  //     Url = _storageConfiguration.BaseUrl + _storageConfiguration.BasePath + path.Replace("\\", "/"),
  //     ContentType = "application/octet-stream",
  //     Size = fileStream.Length
  //   };
  //   return uploadedFile;
  // }

  public async Task<UploadedFile> UploadFile(UploadMethod method, Stream fileStream, UploadOptions options)
  {
    var uploader = _uploadMethodResolver.Resolve(method);
    var fileName = options.FileName;
    var directory = options.Directory;
    var contentType = options.ContentType;
    var path = await uploader.UploadAsync(fileStream, fileName, directory);

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

  // public async Task<UploadedImage> LoadImage(string path)
  // {
  //   var storageConfig = _storageConfig.Value;
  //   var absoluteSavePath = Path.Combine(storageConfig.DirPath, path);
  //   using var fs = File.OpenRead(absoluteSavePath);
  //   var info = Image.Identify(fs) ?? throw new InvalidOperationException("Invalid image");
  //   var fileInfo = new FileInfo(absoluteSavePath);

  //   return new UploadedImage
  //   {
  //     Name = fileInfo.Name,
  //     Path = path,
  //     ContentType = $"image/{fileInfo.Extension.Trim('.')}",
  //     Dimension =
  //     {
  //       Width = info.Width,
  //       Height = info.Height
  //     },
  //     Size = fileInfo.Length,
  //     Url = storageConfig.BaseUrl + "/" + path
  //   };
  // }

  // public async Task<UploadedImage> UploadImage(IFormFile file, UploadOptions? options = null)
  // {
  //   if (file == null || file.Length == 0)
  //   {
  //     throw new Exception("File is empty");
  //   }

  //   using var image = await Image.LoadAsync(file.OpenReadStream());
  //   var storageConfig = _storageConfig.Value;

  //   var encoder = new WebpEncoder
  //   {
  //     Quality = 80,
  //     Method = WebpEncodingMethod.BestQuality
  //   };

  //   var fileName = $"{options?.PreferName ?? Guid.NewGuid().ToString()}.webp";
  //   var relativeSaveDir = (options == null || options.PreferDirectory == null) ? "uploads" : options.PreferDirectory;
  //   var relativeSavePath = Path.Combine(relativeSaveDir, fileName);
  //   var absoluteSaveDir = Path.Combine(storageConfig.DirPath, relativeSaveDir);
  //   var absoluteSavePath = Path.Combine(absoluteSaveDir, fileName);

  //   Directory.CreateDirectory(absoluteSaveDir);

  //   using var ms = new MemoryStream();
  //   await image.SaveAsync(ms, encoder);

  //   await using var fs = File.Create(absoluteSavePath);
  //   ms.Position = 0;
  //   await ms.CopyToAsync(fs);

  //   return new UploadedImage
  //   {
  //     Name = fileName,
  //     Path = relativeSavePath,
  //     ContentType = "image/webp",
  //     Dimension =
  //     {
  //       Height = image.Height,
  //       Width = image.Width
  //     },
  //     Size = ms.Length,
  //     Url = storageConfig.BaseUrl + "/" + relativeSavePath
  //   };
  // }

}
