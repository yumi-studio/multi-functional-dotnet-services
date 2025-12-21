using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using YumiStudio.Common.Configurations;

namespace YumiStudio.Common.Helpers;

public class AwsS3Helper
{
  private readonly StorageConfig _storageConfig;
  private readonly IAmazonS3 _s3Client;
  private readonly ILogger<AwsS3Helper> _logger;

  public AwsS3Helper(
    IOptions<StorageConfig> storageConfig,
    ILogger<AwsS3Helper> logger
  )
  {
    _logger = logger;
    _storageConfig = storageConfig.Value;
    _s3Client = new AmazonS3Client(
      new BasicAWSCredentials(_storageConfig.AwsS3.AccessKey, _storageConfig.AwsS3.SecretKey),
      RegionEndpoint.GetBySystemName(_storageConfig.AwsS3.Region)
    );
  }

  public async Task<string?> UploadFileAsync(
    IFormFile file,
    string? filePath
  )
  {
    try
    {
      var fileTransferUtility = new TransferUtility(_s3Client);
      var s3Key = filePath ?? file.FileName;

      using var stream = file.OpenReadStream();
      await fileTransferUtility.UploadAsync(stream, _storageConfig.AwsS3.BucketName, s3Key);

      return s3Key;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Upload file {FileName} to S3 failed", new { file.FileName });
      return null;
    }
  }

  public async Task<string> GetFileUrlAsync(string filePath)
  {
    return await _s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest()
    {
      BucketName = _storageConfig.AwsS3.BucketName,
      Key = filePath,
      Expires = DateTime.UtcNow.AddHours(1)
    });
  }
}
