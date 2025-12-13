using System;
using YumiStudio.YumiDotNet.Domain.Entities;
using YumiStudio.YumiDotNet.Domain.Interfaces;
using YumiStudio.YumiDotNet.Infrastructure.Persistence.DbContexts;

namespace YumiStudio.YumiDotNet.Infrastructure.Repositories;

public class TokenRepository(
  AppDbContext dbContext
) : BaseRepository<Token, Guid>(dbContext), ITokenRepository
{

}
