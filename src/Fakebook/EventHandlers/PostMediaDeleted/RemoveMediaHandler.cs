using System;
using Fakebook.Events;
using Fakebook.Repositories.Interfaces;
using Fakebook.Services.Interfaces;
using MediatR;

namespace Fakebook.EventHandlers.PostMediaDeleted;

public class RemoveMediaHandler(
  IPostMediaRepository postMediaRepository,
  IFileUploadService uploadService
) : INotificationHandler<OnPostMediaDeleted>
{
  private readonly IPostMediaRepository _postMediaRepository = postMediaRepository;
  private readonly IFileUploadService _uploadService = uploadService;

  public async Task Handle(OnPostMediaDeleted e, CancellationToken ct)
  {
    foreach (var item in e.MediaItems)
    {
      await _uploadService.DeleteFile(item.Path);
    }
  }
}
