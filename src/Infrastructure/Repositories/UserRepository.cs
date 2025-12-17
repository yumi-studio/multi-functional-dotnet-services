
using YumiStudio.Domain.Interfaces;
using YumiStudio.Domain.Entities;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories;

public class UserRepository(AppDbContext appDbContext) : BaseRepository<User, Guid>(appDbContext), IUserRepository
{
}
