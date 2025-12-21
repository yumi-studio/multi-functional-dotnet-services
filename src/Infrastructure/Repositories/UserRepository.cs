
using YumiStudio.Domain.Interfaces;
using YumiStudio.Domain.Entities;
using YumiStudio.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using YumiStudio.Common.Constants;

namespace YumiStudio.Infrastructure.Repositories;

public class UserRepository(AppDbContext appDbContext) : BaseRepository<User, Guid>(appDbContext), IUserRepository
{
  public async Task<UserExternal?> GetUserExternalAuthAsync(Guid userId, ExternalProviders.Provider provider)
  {
    return await appDbContext.UserExternals
      .Where(ue => ue.UserId == userId && ue.Provider == provider)
      .FirstOrDefaultAsync();
  }
}
