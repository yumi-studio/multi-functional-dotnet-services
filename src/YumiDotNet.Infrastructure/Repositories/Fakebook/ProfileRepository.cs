using System;
using YumiStudio.YumiDotNet.Domain.Entities.Fakebook;
using YumiStudio.YumiDotNet.Domain.Interfaces.Fakebook;
using YumiStudio.YumiDotNet.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.YumiDotNet.Infrastructure.Repositories.Fakebook;

public class ProfileRepository(AppDbContext appDbContext) : BaseRepository<Profile, Guid>(appDbContext), IProfileRepository
{

}
