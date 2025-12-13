using YumiStudio.YumiDotNet.Domain.Entities;

namespace YumiStudio.YumiDotNet.Domain.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
}
