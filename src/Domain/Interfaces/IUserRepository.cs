using YumiStudio.Common.Constants;
using YumiStudio.Domain.Entities;

namespace YumiStudio.Domain.Interfaces;

public interface IUserRepository : IRepository<User, Guid>
{
  public Task<UserExternal?> GetUserExternalAuthAsync(Guid userId, ExternalProviders.Provider provider);
}
