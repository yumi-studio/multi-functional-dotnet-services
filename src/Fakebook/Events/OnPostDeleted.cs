using Fakebook.Models.Entities;
using MediatR;

namespace Fakebook.Events;

public record class OnPostDeleted(Post Post) : INotification
{

}
