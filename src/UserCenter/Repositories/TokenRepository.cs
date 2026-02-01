using System;
using UserApi.Models;
using UserApi.Repositories.Interfaces;

namespace UserApi.Repositories;

public class TokenRepository(
  AppDbContext dbContext
) : BaseRepository<Token, Guid>(dbContext), ITokenRepository
{

}
