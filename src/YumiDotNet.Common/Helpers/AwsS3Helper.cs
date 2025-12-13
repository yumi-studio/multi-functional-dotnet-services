using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace YumiStudio.YumiDotNet.Common.Helpers;

public class AwsS3Helper
{
  private readonly IConfigurationSection _s3Config;
  private readonly IAmazonS3 _s3Client;
  private readonly ILogger<AwsS3Helper> _logger;

  public AwsS3Helper(
    IConfiguration config,
    ILogger<AwsS3Helper> logger
  )
  {
    _logger = logger;
    _s3Config = config.GetSection("Aws:S3");
    _s3Client = new AmazonS3Client(
        new BasicAWSCredentials(_s3Config["AccessKey"], _s3Config["SecretKey"]),
        RegionEndpoint.GetBySystemName(_s3Config["Region"])
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
      await fileTransferUtility.UploadAsync(stream, _s3Config["BucketName"], s3Key);

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
      BucketName = _s3Config["BucketName"],
      Key = filePath,
      Expires = DateTime.UtcNow.AddHours(1)
    });
  }
}
