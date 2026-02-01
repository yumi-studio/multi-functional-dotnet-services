using System;
using Microsoft.EntityFrameworkCore;
using UserApi.Enums;
using UserApi.Models;
using UserApi.Repositories.Interfaces;

namespace UserApi.Repositories;

public class UserRepository(AppDbContext appDbContext) : BaseRepository<User, Guid>(appDbContext), IUserRepository
{
  public async Task<UserExternal?> GetUserExternalAuthAsync(Guid userId, AuthProvider provider)
  {
    return await appDbContext.UserExternals
      .Where(ue => ue.UserId == userId && ue.Provider == provider)
      .FirstOrDefaultAsync();
  }

  public async Task<User?> GetUserByEmail(string email)
  {
    return await _dbSet.Where(u => u.Email == email).FirstOrDefaultAsync();
  }
}
