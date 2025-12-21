using System;
using YumiStudio.Domain.Entities;
using YumiStudio.Domain.Interfaces;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories;

public class UserExternalRepository(AppDbContext appDbContext) : BaseRepository<UserExternal, Guid>(appDbContext), IUserExternalRepository
{
}
