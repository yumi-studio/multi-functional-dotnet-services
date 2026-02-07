using Fakebook.Models.Entities;
using MediatR;

namespace Fakebook.Events;

public record class OnPostMediaDeleted(List<PostMedia> MediaItems) : INotification
{

}
