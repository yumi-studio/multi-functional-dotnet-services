using YumiStudio.Domain.Entities;

namespace YumiStudio.Domain.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
}
