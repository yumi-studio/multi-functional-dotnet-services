using System;
using Microsoft.Extensions.Options;
using YumiStudio.YumiDotNet.Application.Interfaces;
using YumiStudio.YumiDotNet.Common.Configurations;
using YumiStudio.YumiDotNet.Common.Helpers;
using YumiStudio.YumiDotNet.Domain.Entities;
using YumiStudio.YumiDotNet.Domain.Enums;
using YumiStudio.YumiDotNet.Domain.Interfaces;

namespace YumiStudio.YumiDotNet.Application.Services;

public class FileCustomOption
{
  public string? Name { get; set; }
  public string? SaveDirPath { get; set; }
  public Guid UploaderId { get; set; } = Guid.Empty;
}

public class FileUploadService(
  IOptions<StorageConfig> storageConfig,
  AwsS3Helper awsS3Helper,
  IFileUploadRepository fileUploadRepository
) : IFileUploadService
{
  private readonly StorageConfig _storageConfig = storageConfig.Value;
  private readonly AwsS3Helper _awsS3Helper = awsS3Helper;
  private readonly IFileUploadRepository _fileUploadRepository = fileUploadRepository;

  private static FileUploadType GetFileCategory(IFormFile file)
  {
    // Use ContentType (MIME type)
    var contentType = file.ContentType.ToLower();

    if (contentType.StartsWith("image/"))
      return FileUploadType.Image;
    if (contentType.StartsWith("video/"))
      return FileUploadType.Video;
    if (contentType.StartsWith("audio/"))
      return FileUploadType.Audio;

    // Optionally, check file extension as a fallback
    var extension = Path.GetExtension(file.FileName).ToLower();
    var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
    var videoExtensions = new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm" };
    var audioExtensions = new[] { ".mp3", ".wav", ".ogg", ".flac", ".aac" };

    if (imageExtensions.Contains(extension))
      return FileUploadType.Image;
    if (videoExtensions.Contains(extension))
      return FileUploadType.Video;
    if (audioExtensions.Contains(extension))
      return FileUploadType.Audio;

    return FileUploadType.Unknown;
  }

  public async Task<FileUpload> Upload(IFormFile file, FileCustomOption? option = null)
  {
    var fileExtension = Path.GetExtension(file.FileName);
    var fileId = Guid.NewGuid();
    var saveFileName = option == null || option.Name == null ? $"{fileId}{fileExtension}" : option.Name;
    var relativeSaveDirPath = (option == null || option.SaveDirPath == null) ? "" : option.SaveDirPath;
    var saveFilePath = Path.Combine(relativeSaveDirPath, saveFileName);
    var fullFilePath = "";
    var uploaderId = option == null ? Guid.Empty : option.UploaderId;

    if (_storageConfig.Default == StorageConfig.LOCALDIR)
    {
      var storageDirPath = _storageConfig.LocalDir.DirPath;
      var fullDirPath = Path.Combine(storageDirPath, relativeSaveDirPath);
      if (!Directory.Exists(fullDirPath))
      {
        Directory.CreateDirectory(fullDirPath);
      }
      fullFilePath = Path.Combine(storageDirPath, saveFilePath);
      using var stream = new FileStream(fullFilePath, FileMode.Create);
      await file.CopyToAsync(stream);
    }

    if (_storageConfig.Default == StorageConfig.AWSS3)
    {
      var storageDirPath = _storageConfig.AwsS3.DirPath;
      fullFilePath = Path.Combine(storageDirPath, saveFilePath);
      var s3Key = await _awsS3Helper.UploadFileAsync(
        file: file,
        filePath: fullFilePath
      ) ?? throw new Exception("Unable to upload file to S3 storage");
    }

    var fileUpload = new FileUpload
    {
      Id = fileId,
      Name = file.FileName,
      FileType = GetFileCategory(file),
      Path = saveFilePath,
      Mime = file.ContentType,
      UploadedBy = uploaderId
    };

    try
    {
      await _fileUploadRepository.AddAsync(fileUpload);
      await _fileUploadRepository.SaveChangesAsync();
      return fileUpload;
    }
    catch
    {
      // If saved failed, should delete file
      if (_storageConfig.Default == StorageConfig.LOCALDIR)
      {
        File.Delete(fullFilePath);
      }
      throw;
    }
  }

  public async Task<string?> GetFileUrl(string filePath)
  {
    if (_storageConfig.Default == StorageConfig.LOCALDIR)
    {
      return _storageConfig.LocalDir.BaseUrl + "/" + filePath;
    }

    if (_storageConfig.Default == StorageConfig.AWSS3)
    {
      return await _awsS3Helper.GetFileUrlAsync(filePath);
    }

    return null;
  }
}
