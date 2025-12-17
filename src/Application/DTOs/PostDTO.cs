namespace YumiStudio.Application.DTOs;

public class PostDto
{
  public Guid Id { get; set; }
  public required string Content { get; set; }
}
