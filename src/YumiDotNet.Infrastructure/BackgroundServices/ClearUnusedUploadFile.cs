using System;
using YumiStudio.YumiDotNet.Domain.Interfaces;

namespace YumiStudio.YumiDotNet.Infrastructure.BackgroundServices;

public class ClearUnusedUploadFile(
  IServiceProvider _serviceProvider
) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        // Your scheduled job logic here
        Console.WriteLine($"Start cleanup draft files at: {DateTime.Now}");

        using var scope = _serviceProvider.CreateScope();
        var fileUploadRepository = scope.ServiceProvider.GetRequiredService<IFileUploadRepository>();

        var draftFiles = await fileUploadRepository.GetAllDrafts();
        var total = 0;
        if (draftFiles.Any())
        {
          foreach (var item in draftFiles)
          {
            fileUploadRepository.Delete(item);
            total++;
          }
          await fileUploadRepository.SaveChangesAsync();
        }

        Console.WriteLine($"End cleanup {total} draft files at: {DateTime.Now}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error cleanup draft files at: {DateTime.Now}.\n {ex.Message}.\n {ex.StackTrace}");
        throw;
      }
      // Wait for a specific interval (e.g., every 1 hour)
      // await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
      // await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
      await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
    }
  }
}
