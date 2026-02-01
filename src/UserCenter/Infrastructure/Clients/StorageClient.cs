using System;

namespace UserApi.Infrastructure.Clients;

public class StorageClient(
  HttpClient _httpClient
)
{
  public async Task<string> GetFileUrl(string filePath)
  {
    var res = await _httpClient.GetAsync($"/rest/V1/products");
    return res.Content.ToString() ?? "";
  }
}
