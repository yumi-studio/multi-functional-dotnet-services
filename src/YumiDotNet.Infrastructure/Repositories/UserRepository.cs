
using YumiStudio.YumiDotNet.Domain.Interfaces;
using YumiStudio.YumiDotNet.Domain.Entities;
using YumiStudio.YumiDotNet.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.YumiDotNet.Infrastructure.Repositories;

public class UserRepository(AppDbContext appDbContext) : BaseRepository<User, Guid>(appDbContext), IUserRepository
{
}
