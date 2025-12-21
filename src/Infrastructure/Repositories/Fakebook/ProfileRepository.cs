using System;
using YumiStudio.Domain.Entities.Fakebook;
using YumiStudio.Domain.Interfaces.Fakebook;
using YumiStudio.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.Infrastructure.Repositories.Fakebook;

public class ProfileRepository(AppDbContext appDbContext) : BaseRepository<Profile, Guid>(appDbContext), IProfileRepository
{

}
