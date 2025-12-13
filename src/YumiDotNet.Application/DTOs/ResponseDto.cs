using System;

namespace YumiStudio.YumiDotNet.Application.DTOs;

public class ResponseDto<T>
{
  public bool Success { get; set; } = true;
  public string? Message { get; set; } = "Success";
  public T? Data { get; set; }
  public List<string> Errors { get; set; } = [];
}
